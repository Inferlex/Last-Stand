using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

internal class Upgrades
{
    private List<float> upgrades;
    private bool[] unlockedUpgrades = { false, false, false, false, false };

    public Upgrades(List<float> upgs)
    {
        upgrades = upgs;
    }

    public void SetUpgrades(int id, float val)
    {
        upgrades[id] = val;
    }

    public float GetUpgrade(int id) => upgrades[id];

    public void SetUnlocked(int id)
    {
        unlockedUpgrades[id] = true;
    }

    public bool[] GetUnlocked()
    {
        return unlockedUpgrades;
    }
}
