using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineAbility : NetworkBehaviour
{
    private int id;
    public virtual void Activate()
    {
        Debug.Log("override this");
    }

    public virtual void Activate(OnlineHero hero)
    {
        Debug.Log("override this");
    }

    public void SetId(int val)
    {
        id = val;
    }

    public int GetId() { return id; }

    [Rpc(SendTo.Server)]
    public void DestroyRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }
}
