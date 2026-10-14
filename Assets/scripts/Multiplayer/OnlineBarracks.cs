using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class OnlineBarracks : OnlineBuilding
{
    new void Start()
    {
        base.Start();
        //GameEvents.current.CapacitySet(new int[2] { GetId(), 2 }); //moved to onlineprebuilding
    }
}
