using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private CircleCollider2D radius;
    public GameObject blastEffect;

    private void Start()
    {
        radius = gameObject.GetComponent<CircleCollider2D>();
    }

    private IEnumerator Attack()
    {
        Instantiate(blastEffect, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.05f);
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f); //used, so the sound can finish playing
        Destroy(gameObject);
    }

    public void Activate()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        StartCoroutine(Attack());
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<Enemy>())
        {
            target.GetComponent<Enemy>().Weaken();
        }
    }
}
