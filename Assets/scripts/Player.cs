using System;
using System.Collections.Generic;
using Assets.Src.Units;
using UnityEngine;

namespace Assets.Src
{
    internal class Player : MonoBehaviour
    {
        public Camera playerCam;
        public int score;
        public int resources;
        public int spResources;

        public List<Building> buildings;
        public List<Unit> units;
        public static Hero hero;
        public static GameObject PlayerManager;
        public int unitCapacity;
        public int currentCapacity;
    }

}
