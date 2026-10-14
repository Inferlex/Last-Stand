using System;
using System.Collections.Generic;
using Assets.Src.Units;
using UnityEngine;

namespace Assets.Src
{
    internal class OnlinePlayer : MonoBehaviour
    {
        public Camera playerCam;
        public int score;
        public int resources;
        public int spResources;

        public List<OnlineBuilding> buildings;
        public List<OnlineUnit> units;
        public OnlineHero hero;
        public GameObject PlayerManager;
        public int unitCapacity;
        public int currentCapacity;
    }

}
