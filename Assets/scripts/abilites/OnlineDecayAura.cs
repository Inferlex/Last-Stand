using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineDecayAura : MonoBehaviour
{
    public GameObject effect;

    public void Use()
    {
        Instantiate(effect, gameObject.transform);
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().Play();
        StartCoroutine(Stop());
    }

    private IEnumerator Stop()
    {
        yield return new WaitForSeconds(10f);
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if(target)
            if(target.CompareTag("PlayerUnit") && target.GetComponent<OnlineTarget>().GetId() != gameObject.GetComponentInParent<OnlineTarget>().GetId())
            {
                target.GetComponent<OnlineUnit>().WeakenEnter();
            }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target)
            if (target.CompareTag("PlayerUnit") && target.GetComponent<OnlineTarget>().GetId() != gameObject.GetComponentInParent<OnlineTarget>().GetId())
            {
                target.GetComponent<OnlineUnit>().WeakenExit();
            }
    }
}
