using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibleAura : MonoBehaviour
{
    public GameObject effect;

    private Hero hero;

    public void Use()
    {
        Instantiate(effect, gameObject.transform);
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponentInParent<Target>().SetInvincible(true);
        GetComponent<AudioSource>().Play();
        StartCoroutine(Stop());
    }

    public void SetHero(Hero unit)
    {
        hero = unit;
    }

    private IEnumerator Stop()
    {
        yield return new WaitForSeconds(10f);
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponentInParent<Target>().SetInvincible(false);
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if(target)
            if(target.GetComponent<Target>() && target.GetComponent<Target>().GetId() == hero.GetId())
            {
                target.GetComponent<Target>().SetInvincible(true);
            }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target)
            if (target.GetComponent<Target>() && target.GetComponent<Target>().GetId() == hero.GetId())
            {
                target.GetComponent<Target>().SetInvincible(false);
            }
    }
}
