using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class OnlineUnitRadius : MonoBehaviour
{
    [SerializeField]private OnlineUnit unit; //the unit this object is attached to

    private int id; //id for when unit dies and we cant know its id

    private void Update()
    {
        if (unit != null)
        {
            gameObject.transform.position = new Vector2(unit.transform.position.x, unit.transform.position.y);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineTarget>() == null) return;
        if (unit.GetId() == 999 || target.GetComponent<OnlineTarget>().GetId() == 999) return;
        if (target && unit)
            if (target.CompareTag("PlayerUnit") && target.transform.GetComponent<OnlineTarget>().GetId() != unit.GetId() && unit.transform.CompareTag("PlayerUnit"))
            {
                unit.TargetEnter(target.GetComponent<OnlineUnit>());
            }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target.GetComponent<OnlineTarget>() == null) return;
        if (unit.GetId() == 999 || target.GetComponent<OnlineTarget>().GetId() == 999) return;
        if (target && unit) //otherwise it will generate an error
            if (target.CompareTag("PlayerUnit") && target.transform.GetComponent<OnlineTarget>().GetId() != unit.GetId() && unit.transform.CompareTag("PlayerUnit"))
            {
                unit.TargetExit(target.GetComponent<OnlineUnit>());
            }
    }

    public void SetUnit(OnlineUnit selected)
    {
        unit = selected;
    }

    public OnlineUnit GetUnit()
    {
        return unit;
    }

    public void SetRadiusId(int val)
    {
        id = val;
    }

    public int GetRadiusId() { return id; }
}
