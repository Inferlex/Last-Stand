using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class UnitRadius : MonoBehaviour
{
    private Unit unit; //the unit this object is attached to

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
        if (target && unit)
            if (target.CompareTag("PlayerUnit") && target.transform.GetComponent<Unit>().GetId()==99 && unit.transform.CompareTag("PlayerUnit"))
            {
                unit.TargetEnter(target.GetComponent<Enemy>());
            }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (unit) //otherwise it will generate an error
            if (target.CompareTag("PlayerUnit") && target.transform.GetComponent<Unit>().GetId() == 99 && unit.transform.CompareTag("PlayerUnit"))
            {
                unit.TargetExit(target.GetComponent<Enemy>());
            }
    }

    public void SetUnit(Unit selected)
    {
        unit = selected;
    }

    public Unit GetUnit()
    {
        return unit;
    }
}
