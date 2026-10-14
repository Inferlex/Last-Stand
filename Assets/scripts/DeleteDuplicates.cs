using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DeleteDuplicates : MonoBehaviour
{
    void Start()
    {
        if(GetComponent<PlayerInfo>() && FindObjectsOfType<PlayerInfo>().Count() > 1)
        {
            for(int i = 1; i < FindObjectsOfType<PlayerInfo>().Count(); i++)
            {
                Destroy(FindObjectsOfType<PlayerInfo>()[i].gameObject);
            }
        }

        if (GetComponent<NetworkManager>() && FindObjectsOfType<NetworkManager>().Count() > 1)
        {
            Destroy(FindObjectsOfType<NetworkManager>()[1].gameObject);
        }
    }
}
