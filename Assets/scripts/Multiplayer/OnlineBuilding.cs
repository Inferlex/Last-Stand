using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Src.Units
{
    public class OnlineBuilding : OnlineTarget
    {
        private bool activated = true;

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
            //GameEvents.current.BuildCreation(this); //it isnt used anymore?
            GameEvents.current.PointsEarn(new int[2] { points, GetId() });
            gameObject.transform.GetChild(0).SetParent(null);
            StartCoroutine(ApplyToGraph());

        }

        public void SetSelected(bool val) //is building selected?
        {
            if (GetId() == 999 && !activated) return;
            if (val)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 100, 255, 255);
                if (GetComponent<OnlineBase>())
                    GetComponent<OnlineBase>().ShowMovePosition(true);
            }
            else
            {
                //transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
                ChangeShadow();
                if (GetComponent<OnlineBase>())
                    GetComponent<OnlineBase>().ShowMovePosition(false);
            }
        }

        public Role getRole() => role;

        public virtual void Activate(bool val)
        {
            activated = val;
        }

        public bool IsActivated() => activated;

        private void OnTriggerEnter2D(Collider2D target)
        {
            if (target.GetComponent<OnlineUnitRadius>() != null && target.GetComponent<OnlineUnitRadius>().GetRadiusId() != GetId())
            {
                IncreaseUnitsLooking();
            }
        }

        private void OnTriggerExit2D(Collider2D target)
        {
            if (target.GetComponent<OnlineUnitRadius>() != null && target.GetComponent<OnlineUnitRadius>().GetRadiusId() != GetId())
            {
                DecreaseUnitsLooking();
            }
        }

        private IEnumerator ApplyToGraph()
        {
            yield return new WaitForSeconds(0.1f);
            GetComponent<GraphUpdateScene>().Apply();
        }
    }
}