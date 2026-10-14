using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class Barracks : Building
{
    new void Start()
    {
        base.Start();
        GameEvents.current.CapacitySet(2);
    }
}
