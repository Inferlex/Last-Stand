using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

namespace Assets.Src.Controls
{
    internal class OnlineBuildingController
    {
        private OnlineBuilding selectedBuilding;        

        internal void SelectBuilding(OnlineBuilding building)
        {
            Deselect();
            selectedBuilding = building;
            selectedBuilding.SetSelected(true);
            GameEvents.current.ClickInformation(building);
        }

        internal void Deselect()
        {
            if (selectedBuilding != null)
            {
                selectedBuilding.SetSelected(false);
                selectedBuilding = null;
            }
        }

        public OnlineBuilding GetBuilding()
        {
            return selectedBuilding;
        }

        public void SetSpawnPoint(Vector2 position)
        {
            if (selectedBuilding && selectedBuilding.GetComponent<OnlineBase>())
            {
                selectedBuilding.GetComponent<OnlineBase>().SetSpawnPoint(position);
            }
        }
    }

}
