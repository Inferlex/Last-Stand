using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineFreezeBlast : OnlineAbility
{
    public GameObject freezeEffect;
    private OnlineHero hero;

    private IEnumerator Attack()
    {
        TriggerEffectRpc();
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.5f); //so the sound can finish
        DestroyRpc();
    }

    public override void Activate(OnlineHero hero)
    {
        this.hero = hero;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        StartCoroutine(Attack());
    }
  
    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineUnit>() && target.GetComponent<OnlineUnit>().GetId() != hero.GetId())
        {            
            target.GetComponent<OnlineUnit>().TakeDamage(50, hero, false);
            target.GetComponent<OnlineUnit>().FreezeRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerEffectRpc()
    {
        Instantiate(freezeEffect, transform.position, Quaternion.identity);
    }
}
