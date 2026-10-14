using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineElectricArc : OnlineAbility
{
    private int aliveTime = 6;
    private CircleCollider2D radius;
    public GameObject arcBlast;
    private OnlineHero hero;

    private void Start()
    {
        radius = gameObject.GetComponent<CircleCollider2D>();
    }

    private IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GetComponent<Animator>().CrossFade("Explosion", 0, 0);
            GetComponent<AudioSource>().Play();
            TriggerEffectRpc();
            foreach(var unit in Physics2D.OverlapCircleAll(transform.position, 3f))
            {
                if (unit.GetComponent<OnlineUnit>() && unit.GetComponent<OnlineUnit>().GetId() != hero.GetId())
                    unit.GetComponent<OnlineUnit>().TakeDamage(40, hero, false);
            }
            aliveTime--;

            if (aliveTime == 0)
                DestroyRpc();
        }
    }

    public override void Activate(OnlineHero hero)
    {
        this.hero = hero;
        GetComponent<Animator>().CrossFade("Arc", 0, 0);
        transform.GetChild(0).gameObject.SetActive(true);
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<CircleCollider2D>().radius = 3.5f;
        StartCoroutine(Attack());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerEffectRpc()
    {
        Instantiate(arcBlast, transform.position, Quaternion.identity);
    }
}
