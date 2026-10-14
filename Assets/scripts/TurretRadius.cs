using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class TurretRadius : MonoBehaviour
{
    public GameObject turret;

    private void Update()
    {
        if (turret == null)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<Enemy>())
        {
            if(turret.GetComponent<Turret>())
                turret.GetComponent<Turret>().TargetEnter(target.GetComponent<Enemy>());
        }
            
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target.GetComponent<Enemy>())
        {
            if (turret && turret.GetComponent<Turret>())
            turret.GetComponent<Turret>().TargetExit(target.GetComponent<Enemy>());
        }
    }

    public Building GetBuilding()
    {
        if(turret.GetComponent<Building>())
            return turret.GetComponent<Building>();
        return default;
    }
}
