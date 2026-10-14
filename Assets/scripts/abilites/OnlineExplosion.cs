using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineExplosion : OnlineAbility
{
    public GameObject explosionEffect;
    private bool activated = false;

    public override void Activate()
    {
        if (!activated)
        {
            activated = true;
            GetComponent<CircleCollider2D>().radius = 4;
            TriggerEffectRpc();
            GetComponent<SpriteRenderer>().enabled = false;
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.05f);
        GameEvents.current.MineDestroy();
        GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        DestroyRpc();
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineUnit>() && target.GetComponent<OnlineUnit>().GetId() != GetId() && activated)
            target.GetComponent<OnlineUnit>().TakeDamage(120, false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerEffectRpc()
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        GetComponent<AudioSource>().Play();
        GetComponent<OnlineExplosion>().Activate();
    }
}
