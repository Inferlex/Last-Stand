using UnityEngine;
using System.Collections.Generic;
using Assets.Src.Units;
using System.Collections;
using Assets.Src.Controls;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.EventSystems;
using Unity.Netcode;


namespace Assets.Src
{
    public class OnlinePlayerManagerNonUsable : NetworkBehaviour
    {
        /*public Texture2D selBorder;
        //private RaycastHit hit; //used to know what we clicked on
        private bool isDragging = false; //are we holding mouse button?
        private UnitController uc;
        private Vector2 mousePosition; //position of the mouse
        private Vector2 selectPosition; //position of select (also mouse)
        private Player player;
        private Camera playerCam;
        public GameEvents ge;
        private BuildingController bc;
        public List<Blueprint> availablebuildings; //all building blueprints are here
        public List<int> buildingCost; //Cost of buildings
        public List<int> buildingSpCost; //Cost of buildings
        public Text[] text = new Text[8]; //UI text
        public GameObject[] animations = new GameObject[1]; //click animations
        public Hero hero; //make it private in the end;

        private bool isBuilding = false; //Used, so we cant build have more than one blueprint on screen
        private int whatIsBuilding;

        public Button[] buttons; //buttons used in UI
        public List<Button> unitButtons;
        public List<Button> StackButtons;
        private Target selectedUnit;
        public PauseMenu pauseMenu;

        public Texture2D cursor;

        public AudioManager am;

        public Camera minimapCamera;

        private int id = 0;

        [SerializeField] private List<string> buttonText;

        [SerializeField] private bool faction; //false - humans, true - aliens

        private void OnGUI() //draws selection rectangle
        {
            if (isDragging)
            {
                uc.DrawSelectionBorder(selectPosition, Input.mousePosition, selBorder);
            }
        }

        private void Start()
        {
            hero = Instantiate(Player.hero); //add in the end when you build 
            animations[0] = Instantiate(animations[0], transform.position, Quaternion.identity);
            for(int i = 0; i < buttons.Length; i++)
            {
                buttons[i].transform.GetChild(0).GetComponent<Text>().text = buttonText[i].Split(" | ")[0];
            }
            //Upgrades.ResetUpgrades();
            //UpgradeStation.ResetUpgrades();
            GameEvents.current.OnBuildCreation += OnBuildCreation; //registering on GameEvents
            GameEvents.current.OnPointsEarn += OnPointsEarn;
            GameEvents.current.OnUnitCreation += OnUnitCreation;
            GameEvents.current.OnResourceEarn += OnResourceEarn;
            GameEvents.current.OnSpResourceEarn += OnSpResourceEarn;
            GameEvents.current.OnClickInformation += OnClickInformation;
            GameEvents.current.OnStartBuilding += OnStartBuilding;
            GameEvents.current.OnShowMessage += OnShowMessage;
            GameEvents.current.OnDeath += OnDeath;
            GameEvents.current.OnCapacitySet += OnCapacitySet;
            GameEvents.current.OnNewWave += OnNewWave;
            GameEvents.current.OnGameFinished += OnGameFinished;
            GameEvents.current.OnHeroDeath += OnHeroDeath;
            GameEvents.current.OnHeroRevived += OnHeroRevived;
            GameEvents.current.OnMinimapClick += OnMinimapClick;
            GameEvents.current.OnCreatingUnit += OnCreatingUnit;
            GameEvents.current.OnBaseCreation += OnBaseCreation;
            ShowButtons(8, 16);
            player = transform.GetChild(0).GetComponent<Player>();
            playerCam = player.playerCam;
            uc = new UnitController(playerCam);
            uc.InitializeGroups();
            bc = new BuildingController();
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        }

        private void OnNewWave(string wave) => text[10].text = wave;

        private void OnBuildCreation(Building building) => player.buildings.Add(building); //adds building to player building list

        private void OnUnitCreation(Unit unit) //adds unit to player unit list
        {
            player.units.Add(unit);
            if (unit.GetComponent<Hero>())
            {
                buttons[15].gameObject.SetActive(true);
            }
        }

        private void OnPointsEarn(int points) //adds score to player
        {
            player.score += points;
            text[5].text = "Score: " + player.score;
        }

        private void OnResourceEarn(int points) //adds resources to player
        {
            player.resources += points;
            text[3].text = player.resources.ToString();
        }

        private void OnSpResourceEarn(int points) //adds resources to player
        {
            player.spResources += points;
            text[4].text = player.spResources.ToString();
        }

        private void OnClickInformation(Target target) //shows information about selected target
        {
            selectedUnit = target;
            text[0].text = selectedUnit.targetName;
            text[1].text = selectedUnit.GetHp() + "/" + selectedUnit.maxHp + " hp";
            if (selectedUnit.GetComponent<Unit>())
            {
                text[6].text = "Attack: " + selectedUnit.GetComponent<Unit>().damage.ToString();
                if (selectedUnit.GetComponent<Hero>())
                {
                    text[7].text = "Level: " + selectedUnit.GetComponent<Hero>().GetLevel().ToString();
                }
            }
        }

        private void OnShowMessage(string message) //displays message
        {
            text[2].text = message;
            StartCoroutine(WaitToFade());
        }

        private void OnStartBuilding(bool value) //we can make another blueprint now
        {
            isBuilding = false;
            if (value)
            {
                player.resources -= buildingCost[whatIsBuilding];
                player.spResources -= buildingSpCost[whatIsBuilding];
                OnResourceEarn(0); //just to update the UI
                OnSpResourceEarn(0); //just to update the UI
            }

        }

        private void OnCapacitySet(int value)
        {
            player.unitCapacity += value;
            text[8].text = player.currentCapacity + " / " + player.unitCapacity;
        }

        private void OnDeath(Target target) //on death of a target
        {
            if (target.CompareTag("PlayerUnit"))
            {
                uc.DeselectUnit(target.GetComponent<Unit>()); //removes unit from selection if there is
                player.units.Remove(target.GetComponent<Unit>());
                player.currentCapacity -= target.GetComponent<Unit>().capacityCost;
                text[8].text = player.currentCapacity + " / " + player.unitCapacity;
                if (target.GetComponent<Hero>())
                {
                    buttons[5].gameObject.SetActive(false);
                }
            }

            if (target.CompareTag("PlayerBuilding"))
                player.buildings.Remove(target.GetComponent<Building>());

            if (target.targetName == "Base" || target.targetName == "Hivemind")
            {
                bool haveBase = false;
                foreach (Building building in player.buildings)
                {
                    if (building.targetName == "Base" && building != target)
                    {
                        haveBase = true;
                    }
                }
                if (!haveBase)
                {
                    OnGameFinished(false);
                }
            }
        }

        private void ShowButtons(int first, int last)
        {
            foreach (Button button in buttons)
            {
                button.gameObject.SetActive(false);
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
            if (!pauseMenu.IsPaused())
            {
                if (selectedUnit != null && (!selectedUnit.GetComponent<Hero>() || !selectedUnit.GetComponent<Hero>().GetIsDead()))
                {
                    OnClickInformation(selectedUnit);
                    if (selectedUnit.GetComponent<Hero>())
                    {
                        ShowButtons(5, 7);
                    }
                    else if (selectedUnit.GetComponent<Unit>())
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
                    if (Physics2D.Raycast(mousePosition, Vector2.zero))
                    {
                        var selected = Physics2D.Raycast(mousePosition, Vector2.zero); //clicked object;
                        var shiftMod = Input.GetKey(KeyCode.LeftShift); //holding shift
                        if (selected.transform.CompareTag("PlayerUnit"))
                        {
                            if (selected.transform.GetComponent<Unit>().GetId() == id)
                            {
                                bc.Deselect();
                                uc.SelectUnit(selected.transform.GetComponent<Unit>(), shiftMod);
                                if (selected.transform.GetComponent<Hero>())
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
                                GameEvents.current.ClickInformation(selected.transform.GetComponent<Target>());
                            }

                        }
                        else if (selected.transform.CompareTag("PlayerBuilding"))
                        {
                            uc.Deselect();
                            bc.SelectBuilding(selected.transform.GetComponent<Building>());
                            BuildingSelected(selected.transform.GetComponent<Building>()); //what building we selected
                        }
                        else if (selected.transform.CompareTag("Menu"))
                        {
                            //when we click on UI, it shouldn't do anything
                        }
                        else
                        {
                            EmptyText();
                            isDragging = true; //holding mouse
                            ShowButtons(8, 16); //shows main buttons
                        }
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
                        if (selected.transform.CompareTag("PlayerUnit") && selected.transform.GetComponent<Unit>().GetId() != id)
                        {
                            animations[0].transform.position = mousePosition;
                            animations[0].GetComponent<Animator>().CrossFade("Attack", 0, 0);
                            uc.AttackWithUnits(selected.transform.GetComponent<Target>());
                        }
                        else if (selected.transform.CompareTag("PlayerBuilding"))
                        {
                            animations[0].transform.position = mousePosition;
                            animations[0].GetComponent<Animator>().CrossFade("Repair", 0, 0);
                            uc.Repair(selected.transform.GetComponent<Building>());
                        }
                        else if (selected.transform.CompareTag("Map"))
                        {
                            animations[0].transform.position = mousePosition;
                            animations[0].GetComponent<Animator>().CrossFade("Destination", 0, 0);
                            uc.MoveUnits(selected.point);
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
                GroupSelect(0, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GroupSelect(1, Input.GetKey(KeyCode.LeftShift));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GroupSelect(2, Input.GetKey(KeyCode.LeftShift));
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

        private void BuildingSelected(Building building) //when building is clicked on
        {
            if (building.getRole() == Building.Role.Base)
            {
                ShowButtons(0, 2);
                if (hero.GetIsDead())
                    ShowHeroButton();
                ShowUnitStackButtons();
            }
            if (building.getRole() == Building.Role.Factory)
            {
                ShowButtons(3, 4);
            }
            if (building.getRole() == Building.Role.UpgradeStation1)
            {
                ShowButtons(17, 19);
            }
            if (building.getRole() == Building.Role.UpgradeStation2)
            {
                ShowButtons(21, 22);
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
                    Instantiate(availablebuildings[i], transform);
                }
                else
                {
                    text[2].text = "not enough resources";
                    StartCoroutine(WaitToFade());
                }
            }
        }

        public IEnumerator WaitToFade() //erases a message after a set time
        {
            yield return new WaitForSeconds(2f);
            text[2].text = "";
        }

        public void CreateUnit(int i) //creates unit
        {
            if (bc.GetBuilding() && bc.GetBuilding().GetComponent<Base>())
            {
                bc.GetBuilding().GetComponent<Base>().CreateUnit(i, player.resources, player.spResources, player.unitCapacity, player.currentCapacity);
            }
        }

        public void ReviveHero()
        {
            if (bc.GetBuilding() && bc.GetBuilding().getRole() == Building.Role.Base)
            {
                bc.GetBuilding().GetComponent<Base>().ReviveHero(hero, player.resources, player.spResources);
            }
        }

        public void OnHeroRevived(Vector3 position)
        {
            hero.gameObject.SetActive(true);
            hero.transform.position = position;
            player.units.Add(hero);
            hero.SetHp(hero.maxHp);
            hero.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            hero.ResetMaterial();
            hero.ResetCooldowns();
            hero.ResetIsAttacking();
            hero.SetIsDead(false);
            hero.dest.position = hero.transform.position;
            hero.healthBar.ChangeSprites(hero.maxHp, hero.GetHp());
            buttons[20].gameObject.SetActive(false);
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
                uc.UseAbility(index, hero);
        }

        public void Upgrade(int index)
        {
            if (bc.GetBuilding() && bc.GetBuilding().GetComponent<UpgradeStation>())
            {
                //bc.GetBuilding().GetComponent<UpgradeStation>().Upgrade(index, player.resources, player.spResources);
            }
        }

        //public void ButtonHoverEnter(string message) => text[9].text = message;
        public void ButtonHoverEnter(int val)
        {
            text[9].text = buttonText[val].Split(" | ")[1];
        }
        public void PowerButtonHoverEnter(int index) => text[9].text = hero.GetPowerDescription(index);

        public void ButtonHoverExit() => text[9].text = "";

        public void ShowInformationAboutUnit(int value) => selectedUnit = uc.GetUnit(value);

        public void CancelUnit(int value) => bc.GetBuilding().GetComponent<Base>().RemoveAt(value);

        public void ShowUnitStackButtons()
        {
            int i = 0;
            foreach (Button button in StackButtons)
            {
                if (bc.GetBuilding() && bc.GetBuilding().GetComponent<Base>() && i < bc.GetBuilding().GetComponent<Base>().GetUnitStack().Count)
                {
                    button.gameObject.SetActive(true);
                    button.transform.GetChild(0).GetComponent<Text>().text = bc.GetBuilding().GetComponent<Base>().GetUnit(i).targetName;
                    if (i == 0)
                    {
                        button.transform.GetChild(0).GetComponent<Text>().text += " " + bc.GetBuilding().GetComponent<Base>().GetProgression() + "%";
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

        public Target GetSelectedUnit() => selectedUnit;

        public int GetUnitCount() => uc.UnitCount();

        public List<Unit> GetGroup(int index) => uc.GetGroup(index);

        private void OnGameFinished(bool win)
        {
            if (win)
            {
                text[11].text = "You have survived!";
            }
            else
            {
                text[11].text = "You were defeated!";
            }
        }

        private void OnHeroDeath()
        {
            player.units.Remove(hero);
            hero.SetIsDead(true);
        }

        public Hero GetHero() => hero;

        public void OnMinimapClick(Vector2 vec) => uc.MoveUnits(vec);

        public int GetScore() => player.score;

        public void OnCreatingUnit(int capacityCost)
        {
            player.currentCapacity += capacityCost;
            text[8].text = player.currentCapacity + " / " + player.unitCapacity;
        }

        public void OnBaseCreation(Base building)
        {
            building.unitList.Add(hero);
        }*/
    }
}