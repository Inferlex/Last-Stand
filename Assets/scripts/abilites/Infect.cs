using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infect : MonoBehaviour
{
    private Hero hero;

    public void Activate()
    {
        GetComponent<Animator>().Play("Infect", 0);
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        Vector2 position = transform.position;
        foreach (var unit in Physics2D.OverlapCircleAll(transform.position, 2f))
        {
            if (unit.GetComponent<Unit>() && unit.GetComponent<Unit>().GetId() != hero.GetId())
                unit.GetComponent<Unit>().Infect(hero.GetId(), hero.GetComponent<Heretic>().GetExplosion());
        }
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }

    public void SetHero(Hero unit)
    {
        hero = unit;
    }
}
