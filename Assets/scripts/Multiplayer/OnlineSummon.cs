using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class OnlineSummon : OnlineUnit
{
    private int selfDamage;

    new void Start()
    {
        if(IsOwner)
            StartCoroutine(StartDying());
        selfDamage = maxHp / 20;
        //transform.SetParent(null);
        base.Start();
    }

    public IEnumerator StartDying()
    {
        yield return new WaitForSeconds(1f);
        SetHealthRpc(GetHp() - selfDamage);
        if(GetHp() <= 0)
        {
            DieRpc();
        }
        StartCoroutine(StartDying());
    }

}
