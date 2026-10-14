using Assets.Src;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreateOnlinePlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerManager;
    [SerializeField] private OnlineHero hero;
    private static int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        //OnlinePlayer.hero = hero;
        GameObject pm = Instantiate(playerManager, new Vector3(transform.position.x+(index*5), transform.position.y + (index * 5), transform.position.z), Quaternion.identity);
        pm.GetComponent<OnlinePlayerManager>().SetId((int)OwnerClientId);
        //CreateManagerClientRpc();
        Destroy(this);
        index++;
    }

    /*[ClientRpc]
    private void CreateManagerClientRpc()
    {
        Instantiate(playerManager, new Vector3(transform.position.x + (index * 5), transform.position.y + (index * 5), transform.position.z), Quaternion.identity);
    }*/
}
