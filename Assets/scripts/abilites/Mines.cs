using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mines : MonoBehaviour
{
    private Hero hero;

    public void Activate()
    {
        GetComponent<CircleCollider2D>().enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<Enemy>())
        {
            StartCoroutine(TargetFound());
        }
    }

    public void SetHero(Hero unit)
    {
        hero = unit;
        GetComponent<Explosion>().SetHero(hero);
    }

    private IEnumerator TargetFound()
    {
        GetComponent<CircleCollider2D>().radius = 0;
        yield return new WaitForSeconds(0.5f);
        GetComponent<AudioSource>().Play();
        GetComponent<Explosion>().Activate();
        enabled = false;
    }
}
