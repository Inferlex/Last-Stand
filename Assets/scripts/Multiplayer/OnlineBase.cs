using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Unity.Netcode;

namespace Assets.Src.Units
{
    public class OnlineBase : OnlineBuilding
    {
        public List<OnlineUnit> unitList;
        private Queue<OnlineUnit> unitStack = new Queue<OnlineUnit>();
        public List<int> unitCost;
        public List<int> unitSpCost;
        public List<float> unitCreationTime;
        private int progression = 0;
        private bool isMaking = false;
        private bool destroyed = false;
        [SerializeField]
        private GameObject movePosition;
        public GameObject buildingArea;
        private int income = 10;
        private List<Button> buttonList = new List<Button>(3);
        [SerializeField] private List<string> buttonTexts;

        static bool heroCreating;

        public new void Start()
        {
            GameEvents.current.OnOnlineBlueprintStart += OnBlueprintStart;
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
            if (!IsActivated()) return;
            if ((res >= unitCost[i] && spRes >= unitSpCost[i]) && currentCapacity + unitList[i].capacityCost <= unitCapacity)
            {
                unitStack.Enqueue((unitList[i]));
                GameEvents.current.CreatingUnit(unitList[i].capacityCost);
                GameEvents.current.ResourceEarn(new int[2] { GetId(), -unitCost[i] });
                GameEvents.current.SpResourceEarn(new int[2] { GetId(), -unitSpCost[i] });
            }
            else
            {
                GameEvents.current.ShowMessage("Cannot create units!/" + GetId());
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
                if (unitStack.Peek().GetComponent<OnlineHero>())
                {
                    CreateHeroRpc();
                    unitStack.Dequeue();
                    heroCreating = false;
                }
                else
                {
                    SpawnRpc(index);
                    unitStack.Dequeue();
                }
            }
            isMaking = false;
        }

        [Rpc(SendTo.Server)]
        private void SpawnRpc(int i)
        {
            OnlineUnit unit = Instantiate(unitList[i], transform.GetChild(1).position, Quaternion.identity);
            unit.GetComponent<NetworkObject>().Spawn(true);
            unit.SetNetworkIdRpc(GetId());
            if (movePosition.transform.position != transform.GetChild(1).position)
            {
                //unit.Move(movePosition.transform.position);
                unit.MoveMessage(movePosition.transform.position);
                unit.SetBoidOn();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void CreateHeroRpc()
        {
            GameEvents.current.OnlineHeroRevived(this);
        }

        public int GetProgression()
        {
            return progression;
        }

        public List<OnlineUnit> GetUnitStack()
        {
            return unitStack.ToList();
        }

        public OnlineUnit GetUnit(int value)
        {
            return unitStack.ToList()[value];
        }

        public void RemoveAt(int value)
        {
            List<OnlineUnit> newStack = unitStack.ToList();
            GameEvents.current.CreatingUnit(-unitList[value].capacityCost);
            GameEvents.current.ResourceEarn(new int[2] { GetId(), unitCost[unitList.IndexOf(newStack[value])] });
            GameEvents.current.SpResourceEarn(new int[2] { GetId(), unitSpCost[unitList.IndexOf(newStack[value])] });

            newStack.RemoveAt(value);
            unitStack.Clear();
            foreach (OnlineUnit unit in newStack)
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
            if(!IsServer)
                SetSpawnPointRpc(position);
        }

        [Rpc(SendTo.Server)]
        public void SetSpawnPointRpc(Vector2 position)
        {
            movePosition.transform.position = position;
        }

        public void OnBlueprintStart(int[] vals)
        {
            if (vals[1] != GetId()) return;
            if (this && getRole() == Role.Base)
                if (vals[0] == 1)
                {
                    buildingArea.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                }
                else
                {
                    buildingArea.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
                }
        }

        public void ReviveHero(OnlineHero hero, int res, int spRes)
        {
            if (!heroCreating)
            {
                heroCreating = true;
                if (res >= unitCost[3] && spRes >= unitSpCost[3])
                {
                    unitStack.Enqueue((unitList[3]));
                    GameEvents.current.ResourceEarn(new int[2] { GetId(), -unitCost[3] });
                    GameEvents.current.SpResourceEarn(new int[2] { GetId(), -unitSpCost[3] });
                }
                else
                {
                    GameEvents.current.ShowMessage("Cannot create units!/" + GetId());
                }
            }
        }

        public IEnumerator Generate()
        {
            while (true)
            {
                yield return new WaitForSeconds(4f);
                GameEvents.current.ResourceEarn(new int[2] { GetId(), income });
            }
        }

        public void ShowMovePosition(bool show)
        {
            if (show)
                movePosition.GetComponent<SpriteRenderer>().enabled = true;
            else
                movePosition.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
