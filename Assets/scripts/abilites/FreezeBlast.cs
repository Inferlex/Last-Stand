using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeBlast : MonoBehaviour
{
    private CircleCollider2D radius;
    public GameObject freezeEffect;
    private Hero hero;

    private void Start()
    {
        radius = gameObject.GetComponent<CircleCollider2D>();
    }

    private IEnumerator Attack()
    {
        Instantiate(freezeEffect, transform.position, Quaternion.identity);
        //yield return new WaitForSeconds(0.05f); why this here?
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.5f); //so the sound can finish
        Destroy(gameObject);
    }

    public void Activate(Hero hero)
    {
        this.hero = hero;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        StartCoroutine(Attack());
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<Unit>() && target.GetComponent<Unit>().GetId() != hero.GetId())
        {            
            target.GetComponent<Enemy>().TakeDamage(50, hero, false);
            target.GetComponent<Enemy>().Freeze();
        }
    }
}
