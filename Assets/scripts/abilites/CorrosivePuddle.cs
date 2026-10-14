using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrosivePuddle : MonoBehaviour
{
    private int aliveTime = 20;
    private Hero hero;

    private IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.3f);
            foreach (var unit in Physics2D.OverlapBoxAll(transform.position, new Vector2(3.5f, 3.5f), 0))
            {
                if (unit.GetComponent<Unit>() && unit.GetComponent<Unit>().GetId() != hero.GetId())
                    unit.GetComponent<Unit>().TakeDamage(10, hero, true); //upravit na unit
            }
            aliveTime--;

            if (aliveTime == 0)
                Destroy(gameObject);
        }
    }

    public void Activate(Hero hero)
    {
        this.hero = hero;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
        StartCoroutine(Attack());
    }
}
