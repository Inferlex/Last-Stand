using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineBomb : OnlineAbility
{
    private CircleCollider2D radius;
    public GameObject[] bombEffect;
    public GameObject bombSignal;

    private void Start()
    {
        radius = gameObject.GetComponent<CircleCollider2D>();
    }

    private IEnumerator Attack()
    {        
        TriggerSignalEffectRpc();
        yield return new WaitForSeconds(3f);
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        TriggerEffectRpc();
        yield return new WaitForSeconds(0.1f);
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.8f);
        DestroyRpc();
    }

    public override void Activate()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(Attack());
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<OnlineTarget>())
        {
            target.GetComponent<OnlineTarget>().TakeDamage(400, true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerSignalEffectRpc()
    {
        Instantiate(bombSignal, transform.position, Quaternion.identity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerEffectRpc()
    {
        foreach (GameObject effect in bombEffect)
        {
            Instantiate(effect, transform.position, Quaternion.identity);
        }
    }
}
