using Assets.Src.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public GameObject spike;
    private GameObject creator;
    private List<Vector2> spikeListPosition;

    public void Activate()
    {
        for (int i = 0; i < 4; i++)
        {
            Instantiate(spike, spikeListPosition[i], Quaternion.identity).GetComponent<SplashDamage>().Activate(70, creator.GetComponent<Unit>().GetId());
        }
    }

    public void SetCreator(GameObject unit)
    {
        creator = unit;
        Vector2 myPosition = transform.position;
        float deltaX = myPosition.x - creator.transform.position.x;
        float deltaY = myPosition.y - creator.transform.position.y ;
        float rad = (float)Math.Atan2(deltaY, deltaX);
        float deg = -rad * (float)(180 / Math.PI);
        Vector2 point = new Vector2(creator.transform.position.x + 5f, creator.transform.position.y);
        point = Quaternion.AngleAxis(deg, creator.transform.position) * point;

        List<Vector2> positionList = new List<Vector2>();
        //Vector2 myPosition = transform.position;
        float x = Mathf.Abs(creator.transform.position.x - point.x) / 4;
        float y = Mathf.Abs(creator.transform.position.y - point.y) / 4;
        if (point.x < creator.transform.position.x)
            x *= -1;
        if (point.y < creator.transform.position.y)
            y *= -1;
        float newValueX = creator.transform.position.x;
        float newValueY = creator.transform.position.y;
        for (int i = 0; i < 4; i++)
        {
            newValueX += x;
            newValueY += y;
            positionList.Add(new Vector2(newValueX, newValueY));
        }
        spikeListPosition = positionList;
        Activate();
    }
}
