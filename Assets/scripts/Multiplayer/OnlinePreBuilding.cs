using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlinePreBuilding : NetworkBehaviour
{
    //private int id;
    public float buildingTime;

    [Rpc(SendTo.ClientsAndHost)]
    public void BuildRpc(int id)
    {
        GetComponent<OnlineBuilding>().Activate(false);
        GetComponent<OnlineBuilding>().SetNetworkIdRpc(id);
        StartCoroutine(Construction(id));
    }

    public IEnumerator Construction(int id)
    {
        yield return new WaitForSeconds(0.01f);
        OnlineBuilding build = GetComponent<OnlineBuilding>();
        if(GetComponent<OnlineTarget>().GetIsOwnedByPlayer())
            GetComponent<SpriteRenderer>().color = new Color32(0, 255, 255, 100);
        build.SetHealthRpc(build.maxHp/10);
        int inc = Mathf.CeilToInt((float)(build.maxHp - build.GetHp())/20);
        for(int i = 0; i < 20; i++)
        {
            if (IsOwner)
            {
                if (build.GetHp() + inc > build.maxHp)
                {
                    build.SetHealthRpc(build.maxHp);
                }
                else
                {
                    build.SetHealthRpc(build.GetHp() + inc);
                }
            }
            
            yield return new WaitForSeconds(buildingTime/20);           
        }
        if (GetComponent<OnlineTarget>().GetIsOwnedByPlayer())
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        //GetComponent<OnlineBuilding>().enabled = true;
        build.Activate(true);
        if(build.getRole() == OnlineBuilding.Role.Barracks)
        {
            GameEvents.current.CapacitySet(new int[2] { id, 2 });
        }
        enabled = false;
    }
}
