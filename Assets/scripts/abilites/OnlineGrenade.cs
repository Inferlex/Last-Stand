using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineGrenade : OnlineAbility
{
    public GameObject blastEffect;

    private IEnumerator Attack()
    {
        TriggerEffectRpc();
        yield return new WaitForSeconds(0.05f);
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f); //used, so the sound can finish playing
        DestroyRpc();
    }    

    public override void Activate()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        StartCoroutine(Attack());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerEffectRpc()
    {
        Instantiate(blastEffect, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineUnit>() && target.GetComponent<OnlineUnit>().GetId() != GetId())
        {
            target.GetComponent<OnlineUnit>().WeakenRpc();
        }
    }
}
