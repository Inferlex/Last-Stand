using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class Summon : Unit
{
    private int selfDamage;

    new void Start()
    {
        StartCoroutine(StartDying());
        selfDamage = maxHp / 20;
        //transform.SetParent(null);
        base.Start();
    }

    public IEnumerator StartDying()
    {
        yield return new WaitForSeconds(1f);
        SetHp(GetHp() - selfDamage);
        if(GetHp() <= 0)
        {
            Die();
        }
        StartCoroutine(StartDying());
    }

}
