using Assets.Src.Units;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Src.Controls
{

    internal class OnlineUnitController
    {
        private Camera cam;
        private List<OnlineUnit> selectedUnits = new List<OnlineUnit>();
        private List<List<OnlineUnit>> groups = new List<List<OnlineUnit>>();
        private int playerId;

        public OnlineUnitController(Camera cam)
        {
            this.cam = cam;
        }

        public void InitializeGroups()
        {
            for (int i = 0; i < 9; i++)
            {
                groups.Add(new List<OnlineUnit>());
            }
        }

        public void MoveUnits(Vector2 pos, TickSystem tickSystem)
        {
            if(selectedUnits.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (OnlineUnit unit in selectedUnits)
                {
                    sb.Append(unit.GetListId());
                    if(unit != selectedUnits.Last())
                    {
                        sb.Append("-");
                    }
                }
                tickSystem.MoveMessage(sb.ToString(), selectedUnits[0], pos);
            }
        }

        /*public void MoveUnits(Vector2 pos, TickSystem tickSystem)
        {
            if (selectedUnits.Count == 1)
            {
                tickSystem.MoveMessage(selectedUnits[0], pos);
            }
            else
            {
                SmartCenter(pos, selectedUnits, tickSystem);
            }
        }*/

        /*public void SmartCenter(Vector2 pos, List<OnlineUnit> units, TickSystem tickSystem)
        {
            List<Vector2> posList = new List<Vector2>();

            float maxDistanceBetweenUnits = 0;
            if (units.Count < 4)
            {
                maxDistanceBetweenUnits = 1.2f;
            }
            else if (units.Count < 7)
            {
                maxDistanceBetweenUnits = 1.8f;
            }
            else
            {
                maxDistanceBetweenUnits = 2.4f;
            }

            foreach (OnlineUnit unit in units)
            {
                posList.Add(unit.transform.position);
            }
            float avgX = 0f;
            float avgY = 0f;
            foreach (Vector2 vector in posList)
            {
                avgX += vector.x;
                avgY += vector.y;
            }
            avgX /= posList.Count;
            avgY /= posList.Count;
            Vector2 newVector = new Vector2(avgX, avgY);
            List<OnlineUnit> leftoverUnits = new List<OnlineUnit>();
            List<OnlineUnit> groupUnits = new List<OnlineUnit>();

            foreach (var unit in units)
            {
                float newX = unit.transform.position.x - avgX;
                float newY = unit.transform.position.y - avgY;
                if ((newX > maxDistanceBetweenUnits || newX < -maxDistanceBetweenUnits) || (newY > maxDistanceBetweenUnits || newY < -maxDistanceBetweenUnits))
                {
                    leftoverUnits.Add(unit);
                }
                else
                {
                    //tickSystem.MoveMessage(unit, new Vector2(pos.x + newX, pos.y + newY));
                    groupUnits.Add(unit);
                }
            }
            if (groupUnits.Count > 1)
            {
                MoveGroup(pos, groupUnits, tickSystem);
            }
            else if (groupUnits.Count == 1)
            {
                leftoverUnits.Add(groupUnits[0]);
            }
            if (leftoverUnits.Count > 0)
            {
                if (leftoverUnits.Count < units.Count)
                {
                    MoveUnitsIter(pos, leftoverUnits, tickSystem);
                }
                else
                {
                    MoveUnitsFar(pos, leftoverUnits, tickSystem);
                }
            }
        }

        private void MoveGroup(Vector2 pos, List<OnlineUnit> units, TickSystem tickSystem)
        {
            List<Vector2> posList = new List<Vector2>();
            foreach (OnlineUnit unit in units)
            {
                posList.Add(unit.transform.position);
            }
            float avgX = 0f;
            float avgY = 0f;
            foreach (Vector2 vector in posList)
            {
                avgX += vector.x;
                avgY += vector.y;
            }
            avgX /= posList.Count;
            avgY /= posList.Count;
            Vector2 avgVector = new Vector2(avgX, avgY);
            tickSystem.CreateGroupAIMessage(units, pos, avgVector);
        }

        private void MoveUnitsIter(Vector2 pos, List<OnlineUnit> units, TickSystem tickSystem)
        {
            if (units.Count == 1)
            {
                tickSystem.MoveMessage(units[0], pos);
            }
            else
            {
                SmartCenter(pos, units, tickSystem);
            }

        }

        private void MoveUnitsFar(Vector2 pos, List<OnlineUnit> units, TickSystem tickSystem)
        {
            int val = units.Count;
            if (val > Fib(5))
            {
                int i = 1;
                while (true)
                {
                    i++;
                    int prevFib = Fib(i - 1);
                    if (Fib(i) >= val)
                    {
                        if (val == Fib(i))
                        {
                            int partOfVal = (int)Mathf.Ceil((float)val / 3);
                            CalculatePos(pos, units.Take(val - partOfVal).ToList(), (float)prevFib / i, tickSystem);
                            MoveUnitsFar(pos, units.Skip(partOfVal).ToList(), tickSystem);
                        }
                        else
                        {
                            CalculatePos(pos, units.Take(prevFib).ToList(), (float)prevFib / i, tickSystem);
                            MoveUnitsFar(pos, units.Skip(prevFib).ToList(), tickSystem);
                        }
                        break;
                    }
                }
            }
            else
            {
                if (val == 1)
                {
                    tickSystem.MoveMessage(units[0], pos);
                }
                else
                {
                    CalculatePos(pos, units, 0.5f, tickSystem);
                }
            }
        }

        public void CalculatePos(Vector2 pos, List<OnlineUnit> units, float mul, TickSystem tickSystem)
        {
            int val = units.Count;
            float rot = 360f / val;
            float rotPoint = 0;

            foreach (var unit in units)
            {
                //unit.Move(new Vector2(pos.x + (Mathf.Cos(rotPoint * (Mathf.PI / 180)) * mul), pos.y + (Mathf.Sin(rotPoint * (Mathf.PI / 180))) * mul));
                tickSystem.MoveMessage(unit, new Vector2(pos.x + (Mathf.Cos(rotPoint * (Mathf.PI / 180)) * mul), pos.y + (Mathf.Sin(rotPoint * (Mathf.PI / 180))) * mul));
                rotPoint += rot;
            }

        }*/

        /*public void SetFlowPath(OnlineFlowFieldGraphManager graph)
        {
            if (selectedUnits.Count != 0)
            {
                foreach (OnlineUnit unit in selectedUnits)
                {
                    unit.SetFlowPath(graph);
                }
            }
        }*/

        static int Fib(int n)
        {
            return (n < 2) ? n : Fib(n - 1) + Fib(n - 2);
        }

        public void AttackWithUnits(OnlineTarget target)
        {
            foreach (var unit in selectedUnits)
            {
                unit.Attack(target);
            }
        }

        public void Repair(OnlineBuilding building)
        {
            foreach (var unit in selectedUnits)
            {
                if (unit.GetComponent<OnlineRepairUnit>())
                    unit.GetComponent<OnlineRepairUnit>().SetRepairingBuilding(building);
            }
        }

        internal void SelectUnit(OnlineUnit unit, bool multiSelect)
        {
            if (multiSelect)
            {
                if (selectedUnits.Contains(unit))
                {
                    selectedUnits.Remove(unit);
                    unit.SetSelected(false);
                }
                else
                {
                    if (selectedUnits.Count < 10)
                    {
                        selectedUnits.Add(unit);
                        unit.SetSelected(true);
                    }
                }
            }
            else
            {
                Deselect();
                selectedUnits.Add(unit);
                unit.SetSelected(true);
            }
            GameEvents.current.ClickInformation(unit);

        }


        internal void GroupSelect(List<OnlineUnit> units, Vector2 mousePos)
        {
            Deselect();
            foreach (var selectableObject in units)
            {
                if (selectedUnits.Count < 10)
                    if (selectableObject != null && IsWithinSelectionBounds(selectableObject.transform, mousePos) && selectableObject.GetId() == playerId)
                    {
                        SelectUnit(selectableObject, true);
                    }
            }
            //GameEvents.current.ClickInformation(units[0]);
        }

        internal void Deselect()
        {
            if (selectedUnits != null)
            {
                foreach (var unit in selectedUnits)
                {
                    unit.SetSelected(false);
                }
                selectedUnits.Clear();
            }
        }

        internal void DeselectUnit(OnlineUnit unit)
        {
            unit.SetSelected(false);
            selectedUnits.Remove(unit);
        }

        internal void DrawSelectionBorder(Vector3 screenPos1, Vector3 screenPos2, Texture2D selBorder)  // Draws rectangle in GUI with a 2D texture
        {
            var borderSize = 2; // In pixels
            var style = new GUIStyle();

            style.border = new RectOffset(borderSize, borderSize, borderSize, borderSize);
            style.normal.background = selBorder;

            GUI.Box(SelectionRect(screenPos1, screenPos2), GUIContent.none, style);
        }

        private static Rect SelectionRect(Vector3 screenPos1, Vector3 screenPos2)   // Generates a rectangle with mouse selection coords
        {
            // Move origin from left to top left
            screenPos1.y = Screen.height - screenPos1.y;
            screenPos2.y = Screen.height - screenPos2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPos1, screenPos2);
            var botRight = Vector3.Max(screenPos1, screenPos2);

            return Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);
        }

        private Bounds GetViewportBounds(Vector3 screenPos1, Vector3 screenPos2)    // Converts camera selection to world coordinates
        {
            var v1 = cam.ScreenToViewportPoint(screenPos1);
            var v2 = cam.ScreenToViewportPoint(screenPos2);

            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);

            min.z = cam.nearClipPlane;
            max.z = cam.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);

            return bounds;
        }

        private bool IsWithinSelectionBounds(Transform transform, Vector2 mousePos)
        {    // Checks is transform is in selection range
            Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            if (IsWithin(transform.position.x, mousePos.x, mousePosition.x) || IsWithin(transform.position.x, mousePosition.x, mousePos.x))
            {
                if (IsWithin(transform.position.y, mousePos.y, mousePosition.y) || IsWithin(transform.position.y, mousePosition.y, mousePos.y))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWithin(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public void UseAbility(int index, OnlineHero hero)
        {
            if (selectedUnits.Contains(hero) && selectedUnits.Count == 1)
            {
                hero.UseAbility(index);
            }
        }

        public void SetGroup(int groupNumber)
        {
            if (selectedUnits.Count > 0)
            {
                groups[groupNumber].Clear();

                foreach (var unit in selectedUnits)
                {
                    groups[groupNumber].Add(unit);
                }
            }

        }

        public void SelectGroup(int groupNumber)
        {
            if (groups[groupNumber] != null && groups[groupNumber].Count > 0)
            {
                Deselect();
                foreach (var unit in groups[groupNumber])
                {
                    if (unit != null)
                        SelectUnit(unit, true);
                }
            }
        }

        public OnlineUnit GetUnit(int value)
        {
            return selectedUnits[value];
        }

        public int UnitCount()
        {
            return selectedUnits.Count;
        }

        public List<OnlineUnit> GetGroup(int index)
        {
            return groups[index];
        }

        public void SetId(int i)
        {
            playerId = i;
        }

        public List<OnlineUnit> GetUnits()
        {
            return selectedUnits;
        }
    }
}