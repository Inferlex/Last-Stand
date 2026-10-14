using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class SplashDamage : MonoBehaviour
{
    int damage;
    //public bool friendly;
    public GameObject splashEffect;
    private int id;


    public void Activate(int value, int unitId)
    {
        damage = value;
        id = unitId;
        GetComponent<CircleCollider2D>().enabled = true;
        Instantiate(splashEffect, transform.position, Quaternion.identity);
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {

            if(target.CompareTag("PlayerUnit") || target.CompareTag("PlayerBuilding"))
            {
                if(target.GetComponent<Target>().GetId() != id)
                    target.GetComponent<Target>().TakeDamage(damage, false);
            }
    }
}
