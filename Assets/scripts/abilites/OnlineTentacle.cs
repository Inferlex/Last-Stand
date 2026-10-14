using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineTentacle : OnlineAbility
{
    private OnlineHero hero;
   
    /*void Start()
    {
        StartCoroutine(Attack());       
    }*/

    public override void Activate(OnlineHero unit)
    {
        hero = unit;
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        GetComponent<Animator>().CrossFade("Reveal", 0, 0);
        yield return new WaitForSeconds(0.6f);
        GetComponent<Animator>().CrossFade("Attack", 0, 0);
        foreach (var unit in Physics2D.OverlapCircleAll(transform.position, 1.4f))
        {
            if (unit.GetComponent<OnlineUnit>() && unit.GetComponent<OnlineUnit>().GetId() != hero.GetId())
                unit.GetComponent<OnlineTarget>().TakeDamage(50, hero, false);
        }
        yield return new WaitForSeconds(0.6f);
        GetComponent<Animator>().CrossFade("Disappear", 0, 0);
        yield return new WaitForSeconds(0.6f);
        if (IsServer)
        {
            DestroyRpc();
        }
    }
}
