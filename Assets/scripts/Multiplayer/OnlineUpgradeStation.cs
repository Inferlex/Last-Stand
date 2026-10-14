using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Assets.Src;

public class OnlineUpgradeStation : OnlineBuilding
{
    public int[] cost;
    public int[] spCost;
    private bool[] upgradeUnlocked;

    private new void Start()
    {
        base.Start();
        if (getRole() == Role.UpgradeStation1)
        {
            upgradeUnlocked = new bool[] { false, false, false };
        }
        else
        {
            upgradeUnlocked = new bool[] { false, false };
        }
    }

    public void Upgrade(int upgradeId, int res, int spRes, OnlinePlayerManager manager)
    {
        if (cost[upgradeId] <= res && spCost[upgradeId] <= spRes)
        {
            if (!upgradeUnlocked[upgradeId])
            {
                upgradeUnlocked[upgradeId] = true;
                GameEvents.current.ResourceEarn(new int[2] { GetId(), -cost[upgradeId] });
                GameEvents.current.SpResourceEarn(new int[2] { GetId(), -spCost[upgradeId] });
                if (!manager.GetFaction())
                {
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
                }
                else
                {
                    switch (upgradeId)
                    {
                        case 0:
                            if (getRole() == Role.UpgradeStation1)
                            {
                                manager.SetUpgrades(0, 25);
                            }
                            else
                            {
                                manager.SetUpgrades(3, 20);
                            }
                                break;
                        case 1:
                            manager.SetUpgrades(1, 1);
                            break;
                        case 2:
                            manager.SetUpgrades(2, 6);
                            break;
                    }
                }
                GameEvents.current.ShowMessage("Upgrade unlocked!/" + GetId());
            }
            else
            {
                GameEvents.current.ShowMessage("Upgrade already unlocked!/" + GetId());
            }
        }
        else
        {
            GameEvents.current.ShowMessage("Not enough resources!/" + GetId());
        }

    }

    /*public static void ResetUpgrades()
    {
        bool[] newUpgradeValues = { false, false, false };
        upgradeUnlocked = newUpgradeValues;
    }*/
    public void SetUpgrades(bool[] upgrades)
    {
        upgradeUnlocked = upgrades;
    }

}
