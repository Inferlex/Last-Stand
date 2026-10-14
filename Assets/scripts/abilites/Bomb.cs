using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private CircleCollider2D radius;
    public GameObject[] bombEffect;
    public GameObject bombSignal;

    private void Start()
    {
        radius = gameObject.GetComponent<CircleCollider2D>();
    }

    private IEnumerator Attack()
    {
        Instantiate(bombSignal, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(3f);
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        foreach(GameObject effect in bombEffect)
        {
            Instantiate(effect, transform.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(0.1f);
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }

    public void Activate()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;        
        StartCoroutine(Attack());
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponent<Target>())
        {
            target.GetComponent<Target>().TakeDamage(400, true);
        }
    }
}
