using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineInfect : OnlineAbility
{
    private OnlineHero hero;

    private void Start()
    {
        PlayAnimation();
    }

    public override void Activate(OnlineHero hero)
    {
        this.hero = hero;
        StartCoroutine(Attack());
    }

    private void PlayAnimation()
    {
        GetComponent<Animator>().Play("Infect", 0);
    }

    private IEnumerator Attack()
    {
        Vector2 position = transform.position;
        foreach (var unit in Physics2D.OverlapCircleAll(transform.position, 2f))
        {
            if (unit.CompareTag("PlayerUnit") && unit.GetComponent<OnlineUnit>().GetId() != GetId() && IsServer)
                unit.GetComponent<OnlineUnit>().Infect(hero.GetId(), hero.GetComponent<OnlineHeretic>().GetExplosion());
        }
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }

    public void SetHero(OnlineHero unit)
    {
        hero = unit;
    }
}
