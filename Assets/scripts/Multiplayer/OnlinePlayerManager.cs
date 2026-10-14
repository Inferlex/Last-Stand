using Assets.Src.Controls;
using Assets.Src.Units;
using Pathfinding.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Assets.Src
{
    public class OnlinePlayerManager : NetworkBehaviour
    {
        public Texture2D selBorder;
        private RaycastHit hit; //used to know what we clicked on
        private bool isDragging = false; //are we holding mouse button?
        private OnlineUnitController uc;
        private Vector2 mousePosition; //position of the mouse
        private Vector2 selectPosition; //position of select (also mouse)
        private OnlinePlayer player;
        private Camera playerCam;
        public GameEvents ge;
        private OnlineBuildingController bc;
        public List<OnlineBlueprint> availablebuildings; //all building blueprints are here
        public List<int> buildingCost; //Cost of buildings
        public List<int> buildingSpCost; //Cost of buildings
        public Text[] text = new Text[8]; //UI text
        public GameObject[] animations = new GameObject[1]; //click animations
        public OnlineHero hero; //make it private in the end;

        private bool isBuilding = false; //Used, so we cant build have more than one blueprint on screen
        private int whatIsBuilding;

        public Button[] buttons; //buttons used in UI
        public List<Button> unitButtons;
        public List<Button> StackButtons;
        private OnlineTarget selectedUnit;
        public PauseMenu pauseMenu;

        public Texture2D cursor;

        public AudioManager am;

        public Camera minimapCamera;

        private int id = 0;

        [SerializeField] private List<string> buttonText;

        //[SerializeField] private OnlineBase playerBase;

        [SerializeField] private GameObject destination;
        private List<OnlineBase> nonIdBases = new List<OnlineBase>();

        private TickSystem tickSystem;
        private Upgrades upg;
        [SerializeField] private bool faction;
        [SerializeField] private OnlineGroupAI groupAIPrefab;
        private int nextId = 0;
        private bool lostGame = false;
        private OnlineFlowFieldGraph ff;
        //private StringBuilder stringBuilder = new StringBuilder();

        private void OnGUI() //draws selection rectangle
        {
            if (isDragging)
            {
                uc.DrawSelectionBorder(selectPosition, Input.mousePosition, selBorder);
            }
        }

        private void Start()
        {
            ff = FindObjectOfType<OnlineFlowFieldGraph>();
            id = (int)OwnerClientId;
            int heroId = FindObjectsOfType<PlayerInfo>()[0].GetHero();
            int baseId = FindObjectsOfType<PlayerInfo>()[0].GetFaction();

            for (int i = 0; i < buttons.Length - 1; i++)
            {
                buttons[i].transform.GetChild(0).GetComponent<Text>().text = buttonText[i].Split(" | ")[0];
            }

            player = transform.GetChild(0).GetComponent<OnlinePlayer>();
            GameEvents.current.OnOnlineUnitCreation += OnUnitCreation; //so the hero could be registered in this event
            GameEvents.current.OnOnlineBuildCreation += OnBuildCreation;
            GameEvents.current.OnOnlineBaseCreation += OnBaseCreation;
            FindObjectOfType<OnlineGameManager>().IncrementPlayersPlaying();
            if (IsOwner)
            {
                FindObjectOfType<OnlineGameManager>().SetPlayer(heroId, id, baseId);
            }

            if (IsOwner)
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }

            animations[0] = Instantiate(animations[0], transform.position, Quaternion.identity);
            if (!faction)
            {
                upg = new Upgrades(new List<float> { 0, 0, 0, 0, 0 });
            }
            else
            {
                upg = new Upgrades(new List<float> { 0, 0, 0, 0 });
            }

            GameEvents.current.OnOnlinePointsEarn += OnPointsEarn;
            GameEvents.current.OnOnlineResourceEarn += OnResourceEarn;
            GameEvents.current.OnOnlineSpResourceEarn += OnSpResourceEarn;
            GameEvents.current.OnOnlineClickInformation += OnClickInformation;
            GameEvents.current.OnStartBuilding += OnStartBuilding;
            GameEvents.current.OnShowMessage += OnShowMessage;
            GameEvents.current.OnOnlineDeath += OnDeath;
            GameEvents.current.OnOnlineCapacitySet += OnCapacitySet;
            GameEvents.current.OnNewWave += OnNewWave;
            GameEvents.current.OnGameFinished += OnGameFinished;
            GameEvents.current.OnOnlineHeroDeath += OnHeroDeath;
            GameEvents.current.OnOnlineHeroRevived += OnHeroRevived;
            GameEvents.current.OnMinimapClick += OnMinimapClick;
            GameEvents.current.OnCreatingUnit += OnCreatingUnit;

            GameEvents.current.OnPreBuilding += OnPreBuilding;
            GameEvents.current.OnMineCreation += OnMinesCreation;

            GameEvents.current.ManagerCreation(this);

            ShowButtons(8, 16);
            //player = transform.GetChild(0).GetComponent<OnlinePlayer>(); is more up now
            player.hero = hero;
            playerCam = FindObjectOfType<Camera>();
            uc = new OnlineUnitController(playerCam);
            uc.InitializeGroups();
            uc.SetId(id);
            bc = new OnlineBuildingController();
            GetOtherPlayers();

            if (IsOwner)
            {
                tickSystem.RegisterMessage("r", id);
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
            }

            if (!hero) //if player doesnt have hero registered, fix it
                StartCoroutine(FixNullHero());
        }

        private void GetOtherPlayers() //dont delete it, it actually helps even when more players are spawned at the same time
        {
            if (IsOwner)
            {
                OnlineTarget[] targets = FindObjectsOfType<OnlineTarget>();
                foreach (OnlineTarget target in targets)
                {
                    if (target.GetId() != id)
                    {
                        target.ChangeColor();
                        target.DeactivateMask();

                        if (target.CompareTag("PlayerUnit")) //collider is disabled on enemy because the id was already changed before client was there
                        {
                            target.GetComponent<CircleCollider2D>().enabled = true;
                        }
                        else if (target.CompareTag("PlayerBuilding"))
                        {
                            target.GetComponent<BoxCollider2D>().enabled = true;
                        }
                    }
                }
            }
        }

        private void OnNewWave(string wave) => text[10].text = wave;

        private void OnPreBuilding(float[] values)
        {
            if ((int)values[3] != id) return;
            SpawnBuildingRpc((int)values[0], values[1], values[2], (int)values[3]);
        }

        private void OnBuildCreation(OnlineBuilding building)
        {
            if (building.GetId() != id)
            {
                if (IsOwner)
                    building.DeactivateMask();
                return;
            }
            if(player.buildings.Count == 0)
            {
                ChangeCameraPos(building);
            }
            player.buildings.Add(building.GetComponent<OnlineBuilding>());
            building.SetListId(nextId);
            nextId++;
            building.GetVisionRadius().GetComponent<OnlineTurretRadius>().SetRadiusId(building.GetId());
            ff.UpdateTilePortals(building);
            if (building.getRole() == OnlineBuilding.Role.Base && building.GetComponent<OnlineBase>().unitList.Count == 3 && hero) //doesnt have hero inside
            {
                building.GetComponent<OnlineBase>().unitList.Add(hero);
            }
        }

        private void OnUnitCreation(OnlineUnit unit) //adds unit to player unit list
        {
            if (unit.GetId() != id)
            {
                if (IsOwner)
                {
                    unit.DeactivateMask();
                }
                return;
            }
            player.units.Add(unit); //this does something weird to hero, its not instance?
            unit.SetListId(nextId);
            nextId++;

            unit.GetVisionRadius().enabled = true;
            unit.GetVisionRadius().GetComponent<OnlineUnitRadius>().SetRadiusId(unit.GetId());


            if (unit.GetComponent<OnlineHero>())
            {
                unit.SetListId(0);
                buttons[15].gameObject.SetActive(true);

                hero = player.units[0].GetComponent<OnlineHero>();
                player.hero = hero;
                hero.SetPowers(buttons.Skip(5).Take(3).ToArray());
                hero.SetButtons();
                /*foreach (OnlineBuilding b in GetPlayerBuildings())
                {
                    if (b.getRole() == OnlineBuilding.Role.Base && !b.GetComponent<OnlineBase>().unitList.Contains(hero))
                    {
                        Debug.Log("add " + hero);
                        b.GetComponent<OnlineBase>().unitList.Add(hero);
                    }
                }*/
            }
            else
            {
                UpgradeUnit(unit);
            }
        }

        private void UpgradeUnit(OnlineUnit unit)
        {
            if (!faction)
            {
                if (unit.targetName == "Drone")
                {
                    unit.maxHp += (int)upg.GetUpgrade(0);
                    unit.SetHealthRpc(unit.maxHp);
                }
                else if (unit.targetName == "Initiate")
                    unit.Upgrade(0, upg.GetUpgrade(1));
                else if (unit.targetName == "Templar")
                    unit.Upgrade(1, upg.GetUpgrade(2));
                else if (unit.targetName == "Walker")
                    unit.Upgrade(3, upg.GetUpgrade(3));
                else if (unit.targetName == "Odin")
                    unit.Upgrade(2, upg.GetUpgrade(4));
            }
            else
            {
                if (unit.targetName == "Bomber")
                {
                    unit.maxHp += (int)upg.GetUpgrade(0);
                    unit.SetHealthRpc(unit.maxHp);
                }
                else if (unit.targetName == "Corrupted")
                    unit.Upgrade(0, upg.GetUpgrade(2));
                else if (unit.targetName == "Destroyer")
                    unit.Upgrade(1, upg.GetUpgrade(3));
                else if (unit.targetName == "Slasher")
                    unit.Upgrade(3, upg.GetUpgrade(1));
            }
        }

        private void OnPointsEarn(int[] points) //adds score to player
        {
            if (points[1] != id) return;
            player.score += points[0];
            text[5].text = "Score: " + player.score;
        }

        private void OnResourceEarn(int[] points) //adds resources to player
        {
            //player.resources += points;
            if (points[0] != id) return;
            player.resources += points[1];
            if (text[3]) //does error otherwise when disconnecting
            {
                text[3].text = player.resources.ToString();
            }
        }

        private void OnSpResourceEarn(int[] points) //adds resources to player
        {
            //player.spResources += points;
            if (points[0] != id) return;
            player.spResources += points[1];
            text[4].text = player.spResources.ToString();
        }

        private void OnClickInformation(OnlineTarget target) //shows information about selected target
        {
            if (text[1] == null) return; //it does error otherwise when disconnecting
            if (target.GetHp() <= 0)
            {
                selectedUnit = null;
                text[0].text = "";
                text[1].text = "";
                text[6].text = "";
                text[7].text = "";
            }
            else
            {
                selectedUnit = target;
                text[0].text = selectedUnit.targetName;
                text[1].text = selectedUnit.GetHp() + "/" + selectedUnit.maxHp + " hp";
                if (selectedUnit.GetComponent<OnlineUnit>())
                {
                    text[6].text = "Attack: " + selectedUnit.GetComponent<OnlineUnit>().damage.ToString();
                    if (selectedUnit.GetComponent<OnlineHero>())
                    {
                        text[7].text = "Level: " + selectedUnit.GetComponent<OnlineHero>().GetLevel().ToString();
                    }
                }
            }
        }

        private void OnShowMessage(string message) //displays message
        {
            string[] splitMessage = message.Split('/');
            if (splitMessage[1] == id.ToString())
            {
                text[2].text = splitMessage[0];
                StartCoroutine(WaitToFade());
            }

        }

        private void OnStartBuilding(bool value) //we can make another blueprint now
        {
            isBuilding = false;
            if (value)
            {
                player.resources -= buildingCost[whatIsBuilding];
                player.spResources -= buildingSpCost[whatIsBuilding];
                OnResourceEarn(new int[2] { id, 0 }); //just to update the UI
                OnSpResourceEarn(new int[2] { id, 0 }); //just to update the UI
            }

        }
        private void OnCapacitySet(int[] value)
        {
            if (value[0] != id) return;
            player.unitCapacity += value[1];
            text[8].text = player.currentCapacity + " / " + player.unitCapacity;
        }

        private void OnDeath(OnlineTarget target) //on death of a target
        {
            if (target.GetId() != id) return;
            if (target.CompareTag("PlayerUnit"))
            {
                uc.DeselectUnit(target.GetComponent<OnlineUnit>()); //removes unit from selection if there is
                player.units.Remove(target.GetComponent<OnlineUnit>());
                player.currentCapacity -= target.GetComponent<OnlineUnit>().capacityCost;
                text[8].text = player.currentCapacity + " / " + player.unitCapacity;
                if (target.GetComponent<OnlineHero>())
                {
                    buttons[5].gameObject.SetActive(false);
                }

                /*for (int i = target.GetListId(); i < player.units.Count; i++) //we want to change id for units with bigger id, as it changed
                {
                    player.units[i].SetListId(i);
                }*/
            }

            if (target.CompareTag("PlayerBuilding"))
            {
                if (target.GetComponent<OnlineBarracks>())
                {
                    GameEvents.current.CapacitySet(new int[2] { target.GetId(), -2 });
                }
                player.buildings.Remove(target.GetComponent<OnlineBuilding>());

                /*for (int i = target.GetListId(); i < player.buildings.Count; i++) //we want to change id for buildings with bigger id, as it changed
                {
                    player.buildings[i].SetListId(i);
                }*/
            }

            if (target.GetComponent<OnlineBuilding>() && target.GetComponent<OnlineBuilding>().getRole() == OnlineBuilding.Role.Base)
            {
                bool haveBase = false;
                foreach (OnlineBuilding building in player.buildings)
                {
                    if (building.getRole() == OnlineBuilding.Role.Base)
                    {
                        haveBase = true;
                    }
                }
                if (!haveBase)
                {
                    lostGame = true;
                    FindObjectOfType<OnlineGameManager>().PlayerDied();
                    OnGameFinished(false);
                }
            }
        }

        private void ShowButtons(int first, int last)
        {
            foreach (Button button in buttons)
            {
                if (button.gameObject.name != "FinishButton")
                {
                    button.gameObject.SetActive(false);
                }
            }
            for (int i = first; i <= last; i++)
            {
                buttons[i].gameObject.SetActive(true);
            }
        }

        private void ShowHeroButton() => buttons[20].gameObject.SetActive(true);

        private void EmptyText()
        {
            selectedUnit = null;
            text[0].text = "";
            text[1].text = "";
            text[6].text = "";
            text[7].text = "";
        }

        private void Update() //Game controls
        {
            if (!IsOwner) return;
            if (!pauseMenu.IsPaused())
            {
                if (selectedUnit != null && selectedUnit.isActiveAndEnabled)
                {
                    OnClickInformation(selectedUnit);
                    if (selectedUnit.GetComponent<OnlineHero>() && selectedUnit.GetComponent<OnlineHero>().GetId() == id && !selectedUnit.GetComponent<OnlineHero>().GetIsDead())
                    {
                        ShowButtons(5, 7);
                    }
                    else if (selectedUnit.GetComponent<OnlineUnit>())
                    {
                        ShowButtons(8, 16);
                    }
                }
                else
                {
                    EmptyText();
                    ShowButtons(8, 16);
                }
                ShowUnitButtons(uc.UnitCount());
                ShowUnitStackButtons();
                if (Input.GetMouseButtonDown(0))    // LMB Down
                {
                    selectPosition = Input.mousePosition; //position of mouse on screen                   
                    mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y); //position of mouse in game

                    var testSelected = Physics2D.Raycast(mousePosition, Vector2.zero);
                    GameObject target = null;
                    if (!testSelected.transform.CompareTag("PlayerUnit") && !testSelected.transform.CompareTag("PlayerBuilding") && !testSelected.transform.CompareTag("Menu"))
                    {
                        foreach (var obj in Physics2D.OverlapCircleAll(mousePosition, 0.2f))
                        {
                            if (obj.transform.CompareTag("PlayerUnit") || obj.transform.CompareTag("PlayerBuilding"))
                            {
                                target = obj.gameObject;
                                break;
                            }

                        }
                    }
                    else
                    {
                        target = testSelected.transform.gameObject;
                    }

                    if (target)
                    {
                        //var selected = Physics2D.Raycast(mousePosition, Vector2.zero); //clicked object;
                        var shiftMod = Input.GetKey(KeyCode.LeftShift); //holding shift
                        if (target.CompareTag("PlayerUnit"))
                        {
                            if (target.GetComponent<OnlineUnit>().GetId() == id)
                            {
                                bc.Deselect();
                                uc.SelectUnit(target.GetComponent<OnlineUnit>(), shiftMod);
                                if (target.GetComponent<OnlineHero>())
                                {
                                    ShowButtons(5, 7);
                                }
                                else
                                {
                                    ShowButtons(8, 16);
                                }
                            }
                            else
                            {
                                GameEvents.current.ClickInformation(target.GetComponent<OnlineTarget>());
                            }

                        }
                        else if (target.CompareTag("PlayerBuilding"))
                        {
                            if (target.GetComponent<OnlineBuilding>().GetId() == id)
                            {
                                uc.Deselect();
                                bc.SelectBuilding(target.GetComponent<OnlineBuilding>());
                                BuildingSelected(target.GetComponent<OnlineBuilding>()); //what building we selected
                            }
                            else
                            {
                                GameEvents.current.ClickInformation(target.GetComponent<OnlineTarget>());
                            }
                        }
                        else if (target.CompareTag("Menu"))
                        {
                            //when we click on UI, it shouldn't do anything
                        }
                    }
                    else
                    {
                        EmptyText();
                        isDragging = true; //holding mouse
                        ShowButtons(8, 16); //shows main buttons
                    }
                }
                else if (isDragging && Input.GetMouseButtonUp(0))   // LMB Release
                {
                    bc.Deselect();
                    uc.GroupSelect(player.units, mousePosition); //or mouse position
                    //ShowUnitButtons(uc.UnitCount());
                    isDragging = false;
                }
                else if (Input.GetMouseButtonDown(1))   // RMB Down
                {
                    mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                    if (Physics2D.Raycast(mousePosition, Vector2.zero))
                    {
                        var selected = Physics2D.Raycast(mousePosition, Vector2.zero);
                        if ((selected.transform.CompareTag("PlayerUnit") || selected.transform.CompareTag("PlayerBuilding")) && selected.transform.GetComponent<OnlineTarget>().GetId() != id && selected.transform.GetComponent<SpriteRenderer>().color != new Color32(255, 255, 255, 0))
                        {
                            animations[0].transform.position = mousePosition;
                            animations[0].GetComponent<Animator>().CrossFade("Attack", 0, 0);
                            //tickSystem.AttackMessage(uc.GetUnits(), selected.transform.GetComponent<OnlineTarget>());
                            tickSystem.AttackMessageGroup(uc.GetUnits(), selected.transform.GetComponent<OnlineTarget>());
                        }
                        else if (selected.transform.CompareTag("PlayerBuilding") && selected.transform.GetComponent<OnlineTarget>().GetId() == id)
                        {
                            animations[0].transform.position = mousePosition;
                            animations[0].GetComponent<Animator>().CrossFade("Repair", 0, 0);
                            tickSystem.RepairMessage(uc.GetUnits(), selected.transform.GetComponent<OnlineBuilding>());
                        }
                        else if (selected.transform.CompareTag("Map"))
                        {
                            animations[0].transform.position = mousePosition;
                            animations[0].GetComponent<Animator>().CrossFade("Destination", 0, 0);
                            uc.MoveUnits(selected.point, tickSystem);
                            //tickSystem.MoveMessage(uc.GetUnits(), selected.point);
                            bc.SetSpawnPoint(selected.point);
                        }
                    }
                }
                else
                {
                    KeyboardPress();
                }
            }
        }

        private void KeyboardPress()
        {
            if (Input.GetKeyDown(KeyCode.B)) //When pushing B
            {
                CreateBuilding(0); //creates base
            }
            else if (Input.GetKeyDown(KeyCode.G)) //When pushing G
            {
                CreateBuilding(1); //creates generator
            }
            else if (Input.GetKeyDown(KeyCode.F)) //When pushing F
            {
                CreateBuilding(2); //creates factory
            }
            else if (Input.GetKeyDown(KeyCode.T)) //When pushing T
            {
                CreateBuilding(3); //creates turret
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                CreateBuilding(4); //creates refinery
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                CreateBuilding(5); //creates refinery
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                SelectHero();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                UseAbility(0);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                UseAbility(1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                UseAbility(2);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                CreateBuilding(6);
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                CreateBuilding(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (bc.GetBuilding())
                {
                    CreateUnit(0);
                }
                else
                {
                    GroupSelect(0, Input.GetKey(KeyCode.LeftShift));
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (bc.GetBuilding())
                {
                    CreateUnit(1);
                }
                else
                {
                    GroupSelect(1, Input.GetKey(KeyCode.LeftShift));
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (bc.GetBuilding())
                {
                    CreateUnit(2);
                }
                else
                {
                    GroupSelect(2, Input.GetKey(KeyCode.LeftShift));
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                GroupSelect(3, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                GroupSelect(4, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                GroupSelect(5, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                GroupSelect(6, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                GroupSelect(7, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                GroupSelect(8, Input.GetKey(KeyCode.LeftShift));
            }
        }

        private void GroupSelect(int number, bool set)
        {
            if (set)
            {
                uc.SetGroup(number);
            }
            else
            {
                uc.SelectGroup(number);
                ShowUnitButtons(uc.UnitCount());
            }
        }

        private void BuildingSelected(OnlineBuilding building) //when building is clicked on
        {
            if (building.getRole() == OnlineBuilding.Role.Base && building.GetId() == id)
            {
                ShowButtons(0, 2);
                if (hero.GetIsDead())
                    ShowHeroButton();
                ShowUnitStackButtons();
            }
            if (building.getRole() == OnlineBuilding.Role.Factory && building.GetId() == id)
            {
                if (!faction)
                {
                    ShowButtons(3, 4);
                }
                else
                {
                    ShowButtons(3, 3);
                }
            }
            if (building.getRole() == OnlineBuilding.Role.UpgradeStation1 && building.GetId() == id)
            {
                ShowButtons(17, 19);
            }
            if (building.getRole() == OnlineBuilding.Role.UpgradeStation2 && building.GetId() == id)
            {
                if (faction)
                {
                    ShowButtons(21, 21);
                }
                else
                {
                    ShowButtons(21, 22);
                }
            }
        }

        public void CreateBuilding(int i) //controls, if we have enough resources to make a building
        {
            if (!isBuilding)
            {
                if (player.resources >= buildingCost[i] && player.spResources >= buildingSpCost[i])
                {
                    uc.Deselect(); //deselects units and buildings when creating blueprint
                    bc.Deselect();
                    isBuilding = true;
                    whatIsBuilding = i;
                    SpawnBlueprint(i);
                }
                else
                {
                    text[2].text = "not enough resources";
                    StartCoroutine(WaitToFade());
                }
            }
        }

        public void SpawnBlueprint(int i)
        {
            OnlineBlueprint bl = Instantiate(availablebuildings[i], transform);
            bl.SetBuildId(i);
            bl.SetPlayerId(id);
        }

        public IEnumerator WaitToFade() //erases a message after a set time
        {
            yield return new WaitForSeconds(2f);
            text[2].text = "";
        }

        public void CreateUnit(int i) //creates unit
        {
            if (bc.GetBuilding() && bc.GetBuilding().GetComponent<OnlineBase>() && bc.GetBuilding().IsActivated())
            {
                bc.GetBuilding().GetComponent<OnlineBase>().CreateUnit(i, player.resources, player.spResources, player.unitCapacity, player.currentCapacity);
            }
        }

        /*public void ReviveHero()
        {
            if (bc.GetBuilding() && bc.GetBuilding().getRole() == OnlineBuilding.Role.Base)
            {
                bc.GetBuilding().GetComponent<OnlineBase>().ReviveHero(hero, player.resources, player.spResources);
            }
        }*/

        public void OnHeroRevived(OnlineBuilding building)
        {
            if (building.GetId() == id)
            {
                //building.GetComponent<OnlineBase>().unitList[3].gameObject.SetActive(true); //in case the hero is not assigned for some reason
                //building.GetComponent<OnlineBase>().unitList[3].GetComponent<OnlineHero>().TeleportToBuilding(building.transform.GetChild(1).position);
                hero.gameObject.SetActive(true);
                hero.TeleportToBuilding(building.transform.GetChild(1).position);
                player.units.Add(hero);
                hero.SetHealthRpc(hero.maxHp);
                hero.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                hero.ResetMaterial();
                hero.ResetCooldowns();
                hero.ResetIsAttacking();
                hero.SetIsDead(false);
                hero.dest.position = hero.transform.position;
                hero.Move(hero.transform.position);
                buttons[20].gameObject.SetActive(false);
                if (!IsOwner)
                {
                    hero.GetComponent<SpriteRenderer>().color = Color.clear;
                }
            }
        }

        public void SelectHero()
        {
            if (!hero.GetIsDead())
            {
                bc.Deselect();
                uc.SelectUnit(hero, false);
            }
        }

        public void UseAbility(int index)
        {
            if (selectedUnit == hero && !isBuilding) //isbuilding is used so the ability cant be used at the same time as you have active blueprint
            {
                uc.UseAbility(index, hero);
            }
        }

        public void Upgrade(int index)
        {
            if (bc.GetBuilding() && bc.GetBuilding().GetComponent<OnlineUpgradeStation>())
            {
                bc.GetBuilding().GetComponent<OnlineUpgradeStation>().Upgrade(index, player.resources, player.spResources, this);
            }
        }

        public void ButtonHoverEnter(int val)
        {
            text[9].text = buttonText[val].Split(" | ")[1];
        }
        public void PowerButtonHoverEnter(int index) => text[9].text = hero.GetPowerDescription(index);

        public void ButtonHoverExit() => text[9].text = "";

        public void ShowInformationAboutUnit(int value) => selectedUnit = uc.GetUnit(value);

        public void CancelUnit(int value) => bc.GetBuilding().GetComponent<OnlineBase>().RemoveAt(value);

        public void ShowUnitStackButtons()
        {
            int i = 0;
            foreach (Button button in StackButtons)
            {
                if (bc.GetBuilding() && bc.GetBuilding().GetComponent<OnlineBase>() && i < bc.GetBuilding().GetComponent<OnlineBase>().GetUnitStack().Count)
                {
                    button.gameObject.SetActive(true);
                    button.transform.GetChild(0).GetComponent<Text>().text = bc.GetBuilding().GetComponent<OnlineBase>().GetUnit(i).targetName;
                    if (i == 0)
                    {
                        button.transform.GetChild(0).GetComponent<Text>().text += " " + bc.GetBuilding().GetComponent<OnlineBase>().GetProgression() + "%";
                    }
                    i++;
                }
                else
                {
                    button.gameObject.SetActive(false);
                }

            }
        }

        public void ShowUnitButtons(int number)
        {

            int i = 0;
            foreach (Button button in unitButtons)
            {
                if (i < number)
                {
                    button.gameObject.SetActive(true);
                    button.transform.GetChild(0).GetComponent<Text>().text = uc.GetUnit(i).targetName;
                    i++;
                }
                else
                {
                    button.gameObject.SetActive(false);
                }

            }
        }

        public OnlineTarget GetSelectedUnit() => selectedUnit;

        public int GetUnitCount() => uc.UnitCount();

        public List<OnlineUnit> GetGroup(int index) => uc.GetGroup(index);

        private void OnGameFinished(bool win)
        {
            if (win && !lostGame)
            {
                text[11].text = "You have won!";
                buttons[23].gameObject.SetActive(true);
            }
            else
            {
                text[11].text = "You were defeated!";
                buttons[23].gameObject.SetActive(true);
            }
        }

        private void OnHeroDeath(int val)
        {
            if (id == val && IsOwner)
            {
                player.units.Remove(hero);
                hero.SetIsDead(true);
            }
        }

        public OnlineHero GetHero() => hero;

        public void OnMinimapClick(Vector2 vec) => uc.MoveUnits(vec, tickSystem);

        public int GetScore() => player.score;

        public void OnCreatingUnit(int capacityCost)
        {
            player.currentCapacity += capacityCost;
            text[8].text = player.currentCapacity + " / " + player.unitCapacity;
        }

        public void OnBaseCreation(OnlineBase building)
        {
            if (building.GetId() == id)
            {
                if (building.unitList.Count == 3)
                {
                    building.unitList.Add(hero);
                }
                if (!player.buildings.Contains(building))
                {
                    player.buildings.Add(building);
                    building.SetListId(nextId);
                    if (player.buildings.Count == 1) //first build will always be the first base
                    {
                        building.SetListId(1);
                        Debug.Log("chanigng cam " + player.playerCam.transform.position);
                        ChangeCameraPos(building);
                        Debug.Log("changed cam " + player.playerCam.transform.position);
                    }
                    nextId++;
                }
            }
            else if (building.GetId() == 999)
            {
                nonIdBases.Add(building);
                if (nonIdBases.Count == 1)
                    StartCoroutine(FixNonIdBases()); //used so that all bases are assigned when they get ids
            }
        }

        public void SetId(int i)
        {
            id = i;
        }

        public int GetId()
        {
            return id;
        }

        [Rpc(SendTo.Server)]
        public void SpawnBuildingRpc(int id, float x, float y, int playerId) //called during prebuilding event
        {
            GameObject building = Instantiate(availablebuildings[id].prefab, new Vector3(x, y, 0f) + new Vector3(0f, 0f, -1f), transform.rotation); //gonna make building now and the building will build itself using prebuilding           
            building.GetComponent<NetworkObject>().Spawn(true);
            building.GetComponent<OnlinePreBuilding>().BuildRpc(playerId);
        }

        private void OnMinesCreation(OnlineMines mine)
        {
            if (id == mine.GetId() && IsOwner)
            {
                mine.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        private IEnumerator FixNonIdBases()
        {

            yield return new WaitForSeconds(0.05f);
            List<OnlineBase> bases = new List<OnlineBase>();
            foreach (OnlineBase nonIdBase in nonIdBases)
            {
                if (nonIdBase.GetId() != 999)
                {
                    if (nonIdBase.GetId() == id && !player.buildings.Contains(nonIdBase))
                    {
                        player.buildings.Add(nonIdBase);
                        nonIdBase.unitList.Add(hero);

                        if (player.buildings.Count == 1) //first build will always be the first base
                        {
                            Debug.Log("chanigng cam " + player.playerCam.transform.position);
                            ChangeCameraPos(nonIdBase);
                            Debug.Log("changed cam " + player.playerCam.transform.position);
                            nonIdBase.SetListId(1);
                        }
                    }
                    bases.Add(nonIdBase);
                }
            }
            foreach (OnlineBase removedBase in bases)
            {
                nonIdBases.Remove(removedBase);
            }
            if (nonIdBases.Count != 0)
            {
                StartCoroutine(FixNonIdBases());
            }
        }

        private IEnumerator FixNullHero()
        {
            yield return new WaitForSeconds(0.05f);

            foreach (OnlineHero h in FindObjectsOfType<OnlineHero>())
            {
                if (h.GetId() == id)
                {
                    hero = h;

                    if (player.units.Count == 0)
                    {
                        player.units.Add(hero);
                        //hero.SetListId(player.units.IndexOf(hero));
                        hero.SetListId(0);
                        nextId++;
                        hero.GetVisionRadius().enabled = true; //in case the radius was disabled before
                        hero.GetVisionRadius().GetComponent<OnlineUnitRadius>().SetRadiusId(hero.GetId());
                        foreach (OnlineBuilding b in GetPlayerBuildings())
                        {
                            if (b.getRole() == OnlineBuilding.Role.Base && !b.GetComponent<OnlineBase>().unitList.Contains(hero))
                            {
                                b.GetComponent<OnlineBase>().unitList.Add(hero);
                            }
                        }
                    }

                    break;
                }
            }

            if (!hero)
            {
                StartCoroutine(FixNullHero());
            }
            if (player.buildings.Count == 2) //sometimes client has duplicates of base
            {
                player.buildings.RemoveAt(1);
            }
            if (player.buildings.Count > 0 && player.buildings[0].GetComponent<OnlineBase>().unitList.Count > 3)
            {
                player.buildings[0].GetComponent<OnlineBase>().unitList[3] = hero;
            }
        }

        public void SetTickSystem(TickSystem ts)
        {
            tickSystem = ts;
        }

        public List<OnlineUnit> GetPlayerUnits()
        {
            return player.units;
        }

        public List<OnlineBuilding> GetPlayerBuildings()
        {
            return player.buildings;
        }

        /*private void WriteIntoFile()
        {
            if (File.Exists("moveLog.txt"))
            {
                var sr = File.AppendText("moveLog.txt");
                sr.Write(stringBuilder.ToString());
                stringBuilder.Clear();
                sr.Close();
                return;
            }
            else
            {
                var sr = File.CreateText("moveLog.txt");
                sr.Write(stringBuilder.ToString());
                stringBuilder.Clear();
                sr.Close();
            }
        }*/

        public void WorkOnMessage(string msg)
        {
            string[] splitMessage = msg.Split("/");
            if (splitMessage.Last() == id.ToString())
            {
                if (splitMessage[0] == "m")
                {
                    /*OnlineUnit unit = FindListId(Int32.Parse(splitMessage[1]), false) as OnlineUnit;
                    if (unit == null)
                    {
                        Debug.Log("aint moving " + splitMessage[1]);
                    }
                    else
                    {
                        //Debug.Log(player.units[int.Parse(splitMessage[1])].targetName + " is moving");
                        //player.units[int.Parse(splitMessage[1])].Move(StringToVector(splitMessage[2]));
                        unit.Move(StringToVector(splitMessage[2]));
                    }*/
                    List<OnlineUnit> units = new List<OnlineUnit>();
                    string[] ids = splitMessage[1].Split("-");
                    foreach (string s in ids)
                    {
                        OnlineUnit unit = FindListId(Int32.Parse(s), false) as OnlineUnit;
                        if (unit != null)
                        {
                            units.Add(unit);
                        }
                    }
                    OnlineFlowFieldGraphManager gm = ff.SetNodePoint(StringToVector(splitMessage[2]), units);
                    if (gm != null)
                    {
                        gm.SetUnitsCount(units.Count);
                        gm.SetLeader(units);
                        foreach (OnlineUnit unit in units)
                        {
                            unit.SetFlowPath(gm);
                        }
                    }

                }
                else if (splitMessage[0] == "a")
                {
                    //OnlineUnit unit = FindListId(Int32.Parse(splitMessage[2]), false) as OnlineUnit;
                    OnlineUnit enemyUnit = tickSystem.GetUnitFromManagerUsingListId(Int32.Parse(splitMessage[4]), Int32.Parse(splitMessage[3]));
                    OnlineBuilding enemyBuilding = tickSystem.GetBuildingFromManagerUsingListId(Int32.Parse(splitMessage[4]), Int32.Parse(splitMessage[3]));
                    OnlineTarget target;
                    //Vector2 targetPos;
                    Node node;
                    if (splitMessage[1] == "u" && enemyUnit)
                    {
                        target = enemyUnit;
                        node = ff.GetNodeAtPosition(target.transform.position);
                    }
                    else if (splitMessage[1] == "b" && enemyBuilding)
                    {
                        target = enemyBuilding;
                        node = ff.GetBuildingNode(target);
                    }
                    else
                    {
                        return;
                    }
                    List<OnlineUnit> units = new List<OnlineUnit>();
                    string[] ids = splitMessage[2].Split("-");
                    foreach (string s in ids)
                    {
                        OnlineUnit unit = FindListId(Int32.Parse(s), false) as OnlineUnit;
                        if (unit != null)
                        {
                            units.Add(unit);
                        }
                    }
                    OnlineFlowFieldGraphManager gm = ff.SetNodePoint(node, units);
                    if (gm != null)
                    {
                        gm.SetUnitsCount(units.Count);
                        gm.SetLeader(units);
                        foreach (OnlineUnit unit in units)
                        {
                            unit.SetAttackQueued(target);
                            unit.SetFlowPathAttack(gm);
                        }
                        gm.SetAttack(target, units);
                        gm.SetUnitsCount(uc.UnitCount());
                        gm.SetLeader(uc.GetUnits());
                    }
                }
                else if (splitMessage[0] == "o")
                {
                    //OnlineUnit unit = FindListId(Int32.Parse(splitMessage[1]), false) as OnlineUnit;
                    /*if (unit == null)
                    {
                        Debug.Log("aint repairing");
                    }
                    else
                    {
                        Debug.Log("repairing");
                        if (unit.GetComponent<OnlineRepairUnit>() != null)
                        {
                            //Debug.Log(player.units[int.Parse(splitMessage[1])].targetName + " is repairing");
                            unit.GetComponent<OnlineRepairUnit>().SetRepairingBuilding(FindListId(Int32.Parse(splitMessage[2]), true) as OnlineBuilding);
                        }
                    }*/
                    OnlineBuilding building = tickSystem.GetBuildingFromManagerUsingListId(Int32.Parse(splitMessage[3]), Int32.Parse(splitMessage[2]));
                    List<OnlineUnit> units = new List<OnlineUnit>();
                    string[] ids = splitMessage[1].Split("-");
                    foreach (string s in ids)
                    {
                        OnlineUnit unit = FindListId(Int32.Parse(s), false) as OnlineUnit;
                        if (unit != null && unit.GetComponent<OnlineRepairUnit>())
                        {
                            units.Add(unit);
                        }
                    }
                    Node node = ff.GetBuildingNode(building);
                    OnlineFlowFieldGraphManager gm = ff.SetNodePoint(node, units);
                    if (gm != null)
                    {

                        gm.SetUnitsCount(units.Count);
                        gm.SetLeader(units);
                        foreach (OnlineUnit unit in units)
                        {
                            unit.SetFlowPath(gm);
                            unit.GetComponent<OnlineRepairUnit>().SetRepairingBuilding(building);
                        }
                    }
                }
                else if (splitMessage[0] == "w")
                {
                    List<OnlineUnit> units = new List<OnlineUnit>();
                    foreach (var splitMsg in splitMessage[3].Split("|"))
                    {
                        OnlineUnit unit = FindListId(Int32.Parse(splitMsg), false) as OnlineUnit;
                        if (unit)
                        {
                            units.Add(unit);
                        }
                    }
                    Instantiate(groupAIPrefab, StringToVector(splitMessage[2]), quaternion.identity).StartNewPath(StringToVector(splitMessage[1]), units);
                }
            }
        }

        public OnlineTarget FindListId(int index, bool type) //type: 0 = unit, 1 = building
        {
            if (type)
            {
                foreach (OnlineBuilding building in player.buildings)
                {
                    if (building.GetListId() == index)
                        return building;
                }
            }
            else
            {
                foreach (OnlineUnit unit in player.units)
                {
                    if (unit.GetListId() == index)
                        return unit;
                }
            }
            return null;
        }


        public Vector2 StringToVector(string msg)
        {
            string[] splitMessage = msg.Split(",");
            //float x = float.Parse(splitMessage[0].Replace('.', ','));
            float x = float.Parse(splitMessage[0], CultureInfo.InvariantCulture);
            //float y = float.Parse(splitMessage[1].Replace('.', ','));
            float y = float.Parse(splitMessage[1], CultureInfo.InvariantCulture);
            /*if (x > 54 || x < -54)
            {
                x /= 100;
            }
            if (y > 54 || y < -54)
            {
                y /= 100;
            }*/
            return new Vector2(x, y);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void DisconnectFromGameRpc(int id)
        {
            if (IsServer)
            {
                if (id == 0)
                {
                    tickSystem.GetComponent<NetworkObject>().Despawn();
                    NetworkManager.Singleton.Shutdown();
                    Destroy(NetworkManager.Singleton);

                }
                else
                {
                    NetworkManager.Singleton.DisconnectClient((ulong)id); // we want the host to disconnect too when that happens
                }
            }
            if (id == 0)
            {
                NetworkManager.Singleton.Shutdown();
                Destroy(NetworkManager.Singleton);
                SceneManager.LoadScene(0);
            }
        }

        public string GetSynchronization(bool sendToOthers)
        {
            StringBuilder sb = new StringBuilder();
            if (sendToOthers)
            {
                sb.Append($"s{id}");
            }
            else
            {
                sb.Append($"x{id}");
            }
            foreach (OnlineUnit unit in player.units)
            {
                string s;
                if (unit.GetAttackQueued() && unit.GetAttackQueued().tag == "PlayerUnit")
                {
                    s = $"{unit.GetAttackQueued().GetId()}-u-{unit.GetAttackQueued().GetListId()}";
                }
                else if (unit.GetAttackQueued() && unit.GetAttackQueued().tag == "PlayerBuilding")
                {
                    s = $"{unit.GetAttackQueued().GetId()}-b-{unit.GetAttackQueued().GetListId()}";
                }
                else
                {
                    s = "n";
                }
                sb.Append($"{player.units.IndexOf(unit)}/{s}/{tickSystem.Simplify(new Vector2(unit.transform.position.x, unit.transform.position.y).ToString())}");
                if (unit != player.units.Last())
                {
                    sb.Append("|");
                }
            }
            //sb.Append(")");
            return sb.ToString();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetMaxNumberOfPlayersRpc(int i)
        {
            if (tickSystem)
            {
                tickSystem.SetMax(i);
            }
            else
            {
                WaitForTickSystem(i);
            }
        }

        private IEnumerator WaitForTickSystem(int i)
        {
            yield return new WaitForSeconds(0.05f);
            SetMaxNumberOfPlayersRpc(i);
        }

        public void SetUpgrades(int id, float val)
        {
            SetUpgradesRpc(id, val);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SetUpgradesRpc(int id, float val)
        {
            upg.SetUpgrades(id, val);
            upg.SetUnlocked(id);
            foreach (var building in player.buildings)
            {
                if (building.getRole() == OnlineBuilding.Role.UpgradeStation1)
                {
                    building.GetComponent<OnlineUpgradeStation>().SetUpgrades(upg.GetUnlocked().Take(3).ToArray());
                }
                else if (building.getRole() == OnlineBuilding.Role.UpgradeStation2)
                {
                    building.GetComponent<OnlineUpgradeStation>().SetUpgrades(upg.GetUnlocked().Skip(3).ToArray());
                }
            }
        }

        private void ChangeCameraPos(OnlineBuilding building)
        {
            if (IsOwner)
            {
                if (playerCam && GetComponent<CameraController>())
                {
                    playerCam.GetComponent<CameraController>().ChangePosition(building.transform.position);
                }
                else
                {
                    FindObjectOfType<Camera>().GetComponent<CameraController>().ChangePosition(building.transform.position);
                }
            }
        }

        public bool GetFaction() => faction;

        public bool GetLost() => lostGame;
    }
}