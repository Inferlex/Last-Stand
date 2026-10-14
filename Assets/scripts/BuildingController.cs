using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

namespace Assets.Src.Controls
{
    internal class BuildingController
    {
        private Building selectedBuilding;        

        internal void SelectBuilding(Building building)
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

        public Building GetBuilding()
        {
            return selectedBuilding;
        }

        public void SetSpawnPoint(Vector2 position)
        {
            if (selectedBuilding && selectedBuilding.GetComponent<Base>())
            {
                selectedBuilding.GetComponent<Base>().SetSpawnPoint(position);
            }
        }
    }

}
