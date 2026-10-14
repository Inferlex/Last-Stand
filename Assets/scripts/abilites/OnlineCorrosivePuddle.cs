using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineCorrosivePuddle : OnlineAbility
{
    private int aliveTime = 20;
    private OnlineHero hero;

    private IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.3f);
            foreach (var unit in Physics2D.OverlapBoxAll(transform.position, new Vector2(3.5f, 3.5f), 0))
            {
                if (unit.GetComponent<OnlineUnit>() && unit.GetComponent<OnlineUnit>().GetId() != hero.GetId())
                    unit.GetComponent<OnlineUnit>().TakeDamage(10, hero, true);
            }
            aliveTime--;

            if (aliveTime == 0)
                Destroy(gameObject);
        }
    }

    public override void Activate(OnlineHero hero)
    {
        this.hero = hero;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
        StartCoroutine(Attack());
        Debug.Log("active puddle");
    }
}
