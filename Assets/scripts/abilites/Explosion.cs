using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GameObject explosionEffect;
    private bool activated = false;
    private Hero hero;

    public void Activate()
    {
        if (!activated)
        {
            activated = true;
            GetComponent<CircleCollider2D>().radius = 4;
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
            GetComponent<SpriteRenderer>().enabled = false;
            StartCoroutine(Die());
        }
    }

    public void SetHero(Hero unit)
    {
        hero = unit;
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.05f);
        GameEvents.current.MineDestroy();
        GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<Unit>() && target.GetComponent<Unit>().GetId() != hero.GetId() && activated)
            target.GetComponent<Unit>().TakeDamage(120, false);
    }
}
