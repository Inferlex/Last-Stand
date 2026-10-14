using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : MonoBehaviour
{
    public GameObject effect;
    private Hero hero;
    private int id;

    public void Use()
    {
        Instantiate(effect, transform.position, Quaternion.identity);
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        StartCoroutine(Stop());
    }

    public void SetHero(Hero unit)
    {
        hero = unit;
    }

    public int GetId() => hero.GetId();

    private IEnumerator Stop()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
    }
}
