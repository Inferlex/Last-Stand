using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Unity.Netcode;

public class OnlineSplashDamage : NetworkBehaviour
{
    int damage;
    //public bool friendly;
    public GameObject splashEffect;
    private int id;
    private OnlineUnit unit;

    private void Start()
    {
        Instantiate(splashEffect, transform.position, Quaternion.identity); // so we dont need to call rpc
    }

    public void Activate(int value, int unitId)
    {
        damage = value;
        id = unitId;
        GetComponent<CircleCollider2D>().enabled = true;      
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.05f);
        //Destroy(gameObject);
        DestroyObjectRpc();
    }

    [Rpc(SendTo.Server)]
    private void DestroyObjectRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    public void SetUnit(OnlineUnit val) => unit = val;

    private void OnTriggerEnter2D(Collider2D target)
    {

            if(target.CompareTag("PlayerUnit") || target.CompareTag("PlayerBuilding"))
            {
            if (target.GetComponent<OnlineTarget>().GetId() != id && IsServer)
            {
                if(unit != null)
                {
                    target.GetComponent<OnlineTarget>().TakeDamage(damage, unit, false);
                }
                else
                {
                    target.GetComponent<OnlineTarget>().TakeDamage(damage, false);
                }
            }
            }
    }
}
