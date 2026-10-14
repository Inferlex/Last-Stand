using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class OnlineTurretRadius : MonoBehaviour
{
    public GameObject turret;
    private int id;

    private void Update()
    {
        if (turret == null)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineTarget>() == null || !turret.GetComponent<OnlineTurret>()) return;
        if (turret.GetComponent<OnlineBuilding>().GetId() == 999 || target.GetComponent<OnlineTarget>().GetId() == 999) return;
        if (turret && target.GetComponent<OnlineTarget>().GetId() != turret.GetComponent<OnlineTarget>().GetId())
        {
            turret.GetComponent<OnlineTurret>().TargetEnter(target.GetComponent<OnlineUnit>());
        }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (turret == null) return;
        if (target != null && target.GetComponent<OnlineTarget>() == null || !turret.GetComponent<OnlineTurret>()) return;
        if (turret.GetComponent<OnlineBuilding>().GetId() == 999 || target.GetComponent<OnlineTarget>().GetId() == 999) return;
        if (target.GetComponent<OnlineTarget>().GetId() != turret.GetComponent<OnlineTarget>().GetId())
        {
            turret.GetComponent<OnlineTurret>().TargetExit(target.GetComponent<OnlineUnit>());
        }
    }

    public OnlineBuilding GetBuilding()
    {
        if(turret && turret.GetComponent<OnlineBuilding>())
            return turret.GetComponent<OnlineBuilding>();
        return default;
    }

    public void SetRadiusId(int val)
    {
        id = val;
    }

    public int GetRadiusId() { return id; }
}
