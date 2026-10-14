using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineInvincibleAura : MonoBehaviour
{
    public GameObject effect;
    private OnlineHero hero;

    public void Use()
    {
        Instantiate(effect, gameObject.transform);
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<CircleCollider2D>().enabled = true;
        //GetComponentInParent<OnlineTarget>().SetInvincible(true);
        hero.SetInvincible(true);
        GetComponent<AudioSource>().Play();
        StartCoroutine(Stop());
    }

    public void SetHero(OnlineHero unit)
    {
        hero = unit;
    }

    private IEnumerator Stop()
    {
        yield return new WaitForSeconds(10f);
        GetComponent<CircleCollider2D>().enabled = false;
        //GetComponentInParent<Target>().SetInvincible(false);
        hero.SetInvincible(false);
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if(target)
            if(target.GetComponent<OnlineTarget>() && target.GetComponent<OnlineTarget>().GetId() == hero.GetId())
            {
                target.GetComponent<OnlineTarget>().SetInvincible(true);
            }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target)
            if (target.GetComponent<OnlineTarget>() && target.GetComponent<OnlineTarget>().GetId() == hero.GetId())
            {
                target.GetComponent<OnlineTarget>().SetInvincible(false);
            }
    }
}
