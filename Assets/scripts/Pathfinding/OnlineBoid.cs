using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;

public class OnlineBoid : MonoBehaviour
{
    private CircleCollider2D circleCollider;
    private bool isOn = false;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        if (isOn) //if its too expensive to scan every frame, we could try every few frames and if we find collision, do it every frame for a while
        {
            Avoid();
        }

    }

    private void Avoid()
    {
        Collider2D[] results = new Collider2D[10];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        contactFilter.maxDepth = 0;
        contactFilter.minDepth = -1;
        circleCollider.OverlapCollider(contactFilter, results);
        results = results.Where(x => x != null).Where(x => x.transform.gameObject.GetComponent<OnlineUnit>()).ToArray();
        foreach (Collider2D c in results)
        {
            if (c.transform != transform.parent)
            {
                GameObject unit = c.transform.gameObject;

                float x = unit.transform.position.x - transform.position.x;
                float y = unit.transform.position.y - transform.position.y;

                transform.parent.transform.position += transform.parent.GetComponent<OnlineUnit>().ApplySpeed(Vector2.Perpendicular(new Vector2(x, y))); // head left of the collision unit
                transform.parent.transform.position += transform.parent.GetComponent<OnlineUnit>().ApplySpeed(new Vector2(-x, -y)); //we also want to get further from unit

                if (transform.parent.GetComponent<OnlineUnit>().GetFlowPath() && Vector2.Distance(transform.parent.GetComponent<OnlineUnit>().GetFlowPath().GetMovePosition(), unit.transform.position) < 0.6f) //if object is in the way but the unit is close anyway, stop
                {
                    transform.parent.GetComponent<OnlineUnit>().PathDone();
                }
            }
        }
    }

    public void SetIsOn(bool val)
    {
        isOn = val;
    }
}
