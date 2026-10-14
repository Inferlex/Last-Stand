using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Assets.Src.Units
{
    public class Building : Target
    {

        public enum Role
        {
            Base,
            Generator,
            Factory,
            UpgradeStation1,
            UpgradeStation2,
            Barracks,
            Refinery,
            Turret
        }

        [SerializeField] private Role role;

        public new void Start()
        {
            base.Start();
            GetComponent<GraphUpdateScene>().Apply();
            GameEvents.current.BuildCreation(this);
            GameEvents.current.PointsEarn(points);
            gameObject.transform.GetChild(0).SetParent(null);
        }

        public void SetSelected(bool val) //is building selected?
        {
            if (val)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 100, 255, 255);
                if (GetComponent<Base>())
                    GetComponent<Base>().ShowMovePosition(true);
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
                if (GetComponent<Base>())
                    GetComponent<Base>().ShowMovePosition(false);
            }
        }

        public Role getRole() => role;
    }

}