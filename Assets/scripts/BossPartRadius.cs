using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPartRadius : MonoBehaviour
{
    public BossPart part;

    private void Start()
    {
        //transform.SetParent(null);
    }

    private void Update()
    {
        if (part == null)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.CompareTag("PlayerUnit") || target.CompareTag("PlayerBuilding"))
        {
            part.TargetEnter(target.GetComponent<Target>());
        }

    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target.CompareTag("PlayerUnit") || target.CompareTag("PlayerBuilding"))
        {
            part.TargetExit(target.GetComponent<Target>());
        }
    }
}
