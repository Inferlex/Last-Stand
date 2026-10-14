using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricArc : MonoBehaviour
{
    private int aliveTime = 6;
    private CircleCollider2D radius;
    public GameObject arcBlast;
    private Hero hero;

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
            Instantiate(arcBlast, transform);
            foreach(var unit in Physics2D.OverlapCircleAll(transform.position, 3f))
            {
                if (unit.GetComponent<Unit>() && unit.GetComponent<Unit>().GetId() != hero.GetId())
                    unit.GetComponent<Enemy>().TakeDamage(40, hero, false);
            }
            aliveTime--;

            if (aliveTime == 0)
                Destroy(gameObject);
        }
    }

    public void Activate(Hero hero)
    {
        this.hero = hero;
        GetComponent<Animator>().CrossFade("Arc", 0, 0);
        transform.GetChild(0).gameObject.SetActive(true);
        GetComponent<CircleCollider2D>().enabled = true;
        radius.radius = 3.5f;
        StartCoroutine(Attack());
    }
}
