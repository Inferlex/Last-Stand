using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Assets.Src;

public class UpgradeStation : Building
{
    //public Unit[] unitList;
    [SerializeField] private int[] cost;
    [SerializeField] private int[] spCost;
    private bool[] upgradeUnlocked;

    private new void Start()
    {
        base.Start();
        if(getRole() == Role.UpgradeStation1)
        {
            upgradeUnlocked = new bool[] { false, false, false };
        }
        else
        {
            upgradeUnlocked = new bool[] { false, false };
        }
    }

    public void Upgrade(int upgradeId, int res, int spRes, PlayerManager manager)
    {
        if (cost[upgradeId] <= res && spCost[upgradeId] <= spRes)
        {
            if (!upgradeUnlocked[upgradeId])
            {
                upgradeUnlocked[upgradeId] = true;
                GameEvents.current.ResourceEarn(-cost[upgradeId]);
                GameEvents.current.SpResourceEarn(-spCost[upgradeId]);
                switch (upgradeId)
                {
                    case 0:
                        if (getRole() == Role.UpgradeStation1)
                        {
                            manager.SetUpgrades(0, 25);
                        }
                        else
                        {
                            manager.SetUpgrades(3, 1);
                        }
                        break;
                    case 1:
                        if (getRole() == Role.UpgradeStation1)
                        {
                            manager.SetUpgrades(1, 8);
                        }
                        else
                        {
                            manager.SetUpgrades(4, 1);
                        }
                        break;
                    case 2:
                        manager.SetUpgrades(2, 20);
                        break;
                }
                GameEvents.current.ShowMessage("Upgrade unlocked!");
            }         
            else
            {
                GameEvents.current.ShowMessage("Upgrade already unlocked!");
            }
        }
        else
        {
            GameEvents.current.ShowMessage("Not enough resources!");
        }

    }

    public void SetUpgrades(bool[] upgrades)
    {
        upgradeUnlocked = upgrades;
    }
}
