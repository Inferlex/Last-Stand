using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineMines : NetworkBehaviour
{
    private int id = 999;

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineTarget>() && target.GetComponent<OnlineTarget>().GetId() != id)
        {
            StartCoroutine(TargetFound());
        }
    }

    public void SetId(int heroId)
    {
        id = heroId;
        GetComponent<CircleCollider2D>().enabled = true;
        SetIdRpc(id);
        GetComponent<OnlineExplosion>().SetId(id);
    }

    public int GetId()
    {
        return id;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetIdRpc(int id)
    {
        this.id = id;
        GameEvents.current.MineCreation(this);
    }

    private IEnumerator TargetFound()
    {
        GetComponent<CircleCollider2D>().radius = 0;
        yield return new WaitForSeconds(0.5f);
        GetComponent<OnlineExplosion>().Activate();
        enabled = false;
    }
}
