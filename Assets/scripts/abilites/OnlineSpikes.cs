using Assets.Src.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineSpikes : NetworkBehaviour
{
    public GameObject spike;
    private GameObject creator;
    private List<Vector2> spikeListPosition = new List<Vector2>();

    public void Activate()
    {
        if (IsServer)
        {
            for (int i = 0; i < 4; i++)
            {
                OnlineSplashDamage ability = Instantiate(spike, spikeListPosition[i], Quaternion.identity).GetComponent<OnlineSplashDamage>();
                ability.GetComponent<NetworkObject>().Spawn();
                ability.SetUnit(creator.GetComponent<OnlineUnit>());
                ability.Activate(70, creator.GetComponent<OnlineUnit>().GetId());

            }
            GetComponent<NetworkObject>().Despawn();
            Destroy(this);
        }
    }

    public void SetCreator(GameObject unit)
    {
        creator = unit;

        Vector2 EndPosition = transform.position;
        Vector2 StartPosition = creator.transform.position;
        float xDivider = (Math.Max(StartPosition.x, EndPosition.x) - Math.Min(StartPosition.x, EndPosition.x)) / 4;
        float yDivider = (Math.Max(StartPosition.y, EndPosition.y) - Math.Min(StartPosition.y, EndPosition.y)) / 4;
        float x = StartPosition.x;
        float y = StartPosition.y;
        if(EndPosition.x < StartPosition.x)
        {
            xDivider *= -1;
        }
        if (EndPosition.y < StartPosition.y)
        {
            yDivider *= -1;
        }

        for (int i = 0; i < 4; i++)
        {
            x += xDivider;
            y += yDivider;
            spikeListPosition.Add(new Vector2(x, y));
        }

        Activate();
    }
}
