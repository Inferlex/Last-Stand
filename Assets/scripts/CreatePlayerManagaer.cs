using Assets.Src;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlayerManagaer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(Player.PlayerManager);
        Destroy(this);
    }
}
