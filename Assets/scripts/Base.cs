using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Assets.Src.Units
{
    public class Base : Building
    {
        public List<Unit> unitList;
        private Queue<Unit> unitStack = new Queue<Unit>();
        public List<int> unitCost;
        public List<int> unitSpCost;
        public List<float> unitCreationTime;
        private int progression = 0;
        private bool isMaking = false;
        private bool destroyed = false;
        [SerializeField]
        private GameObject movePosition;
        //private int[] upgrades = Enumerable.Repeat<int>(0, 3).ToArray();
        public GameObject buildingArea;
        private int income = 10;
        private List<Button> buttonList = new List<Button>(3);
        [SerializeField] private List<string> buttonTexts;

        static bool heroCreating;

        private PlayerManager playerManager; // for flowfield

        public new void Start()
        {
            GameEvents.current.OnBlueprintStart += OnBlueprintStart;
            base.Start();

            movePosition.transform.position = transform.GetChild(1).position;
            if (getRole() == Role.Base)
                GameEvents.current.BaseCreation(this);
            StartCoroutine(Generate());
        }

        void Update()
        {
            if (unitStack.Count != 0 && !isMaking)
            {
                StartCoroutine(MakingUnit());
            }
        }

        public void CreateUnit(int i, int res, int spRes, int unitCapacity, int currentCapacity)
        {
            if ((res >= unitCost[i] && spRes >= unitSpCost[i]) && currentCapacity + unitList[i].capacityCost <= unitCapacity)
            {
                unitStack.Enqueue((unitList[i]));
                GameEvents.current.CreatingUnit(unitList[i].capacityCost);
                GameEvents.current.ResourceEarn(-unitCost[i]);
                GameEvents.current.SpResourceEarn(-unitSpCost[i]);
            }
            else
            {
                GameEvents.current.ShowMessage("Cannot create units!");
            }

        }

        private IEnumerator MakingUnit() //waits until an unit is finished
        {
            int index = unitList.IndexOf(unitStack.Peek());
            progression = 0;
            isMaking = true;
            int i = 0;
            while (i < 100) //used for creation bar
            {
                if (!destroyed)
                {
                    progression += 1;
                    yield return new WaitForSeconds(unitCreationTime[index] / 100f);
                }
                i++;
            }
            if (destroyed) //when unit was canceled production while creating
            {
                destroyed = false;
                isMaking = false;
                progression = 0;
            }
            else
            {
                if (unitStack.Peek().GetComponent<Hero>())
                {
                    GameEvents.current.HeroRevived(transform.GetChild(1).position);
                    unitStack.Dequeue();
                    heroCreating = false;
                }
                else
                {
                    Unit unit = Instantiate(unitStack.Dequeue(), transform.GetChild(1).position, Quaternion.identity);
                    if (movePosition.transform.position != transform.GetChild(1).position)
                    {
                        //unit.dest.position = movePosition.transform.position; //used for normal movement
                        playerManager.SetFlowField(unit, movePosition.transform.position);
                        unit.SetBoidOn();
                    }
                }
            }
            isMaking = false;
        }

        public int GetProgression()
        {
            return progression;
        }

        public List<Unit> GetUnitStack()
        {
            return unitStack.ToList();
        }

        public Unit GetUnit(int value)
        {
            return unitStack.ToList()[value];
        }

        public void RemoveAt(int value)
        {
            List<Unit> newStack = unitStack.ToList();
            GameEvents.current.CreatingUnit(-unitList[value].capacityCost);
            GameEvents.current.ResourceEarn(unitCost[unitList.IndexOf(newStack[value])]);
            GameEvents.current.ResourceEarn(unitSpCost[unitList.IndexOf(newStack[value])]);
            newStack.RemoveAt(value);
            unitStack.Clear();
            foreach (Unit unit in newStack)
            {
                unitStack.Enqueue(unit);
            }
            if (value == 0)
            {
                destroyed = true;
            }
        }

        public void SetSpawnPoint(Vector2 position)
        {
            movePosition.transform.position = position;
        }

        public void OnBlueprintStart(bool start)
        {
            if (this && getRole() == Role.Base)
                if (start)
                {
                    buildingArea.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                }
                else
                {
                    buildingArea.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
                }
        }

        public void ReviveHero(Hero hero, int res, int spRes)
        {
            if (!heroCreating)
            {
                heroCreating = true;
                if (res >= unitCost[3] && spRes >= unitSpCost[3])
                {
                    unitStack.Enqueue((unitList[3]));
                    GameEvents.current.ResourceEarn(-unitCost[3]);
                    GameEvents.current.ResourceEarn(-unitSpCost[3]);
                }
                else
                {
                    GameEvents.current.ShowMessage("Cannot create units!");
                }
            }
        }

        public IEnumerator Generate()
        {
            while (true)
            {
                yield return new WaitForSeconds(4f);
                GameEvents.current.ResourceEarn(income);
            }
        }

        public void ShowMovePosition(bool show)
        {
            if (show)
                movePosition.GetComponent<SpriteRenderer>().enabled = true;
            else
                movePosition.GetComponent<SpriteRenderer>().enabled = false;
        }

        public void SetManager(PlayerManager manager)
        {
            playerManager = manager;
        }
    }  
}
