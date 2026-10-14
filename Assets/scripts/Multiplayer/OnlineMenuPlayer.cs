using Assets.Src;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineMenuPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerManager;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            FindObjectOfType<OnlineSceneManager>().SetMaxNumberOfPlayers(FindObjectOfType<MainMenu>().GetMaxNumberOfPlayers());           
        }
        FindObjectOfType<OnlineSceneManager>().StartGameRpc();
    }

    public void Start()
    {
        SetPlayerManager(FindObjectOfType<PlayerInfo>().GetFaction());
    }

    public void SetPlayerManager(int i)
    {
        if (IsOwner)
        {
            SetPlayerManagerRpc(i);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerManagerRpc(int i)
    {
        playerManager = FindObjectOfType<MainMenu>().faction[i];
    }

    public void StartGame()
    {
        if (IsServer)
        {
            FindObjectOfType<OnlineGameManager>().SetMaxPlayers(GetComponent<OnlineSceneManager>().GetMaxNumberOfPlayers());
        }
        SpawnPlayerManagerRpc();
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerManagerRpc()
    {
        GameObject pm = Instantiate(playerManager);    
        pm.GetComponent<NetworkObject>().Spawn(true);
        if(pm.GetComponent<NetworkObject>().OwnerClientId != GetComponent<NetworkObject>().OwnerClientId)
        {
            pm.GetComponent<NetworkObject>().ChangeOwnership(GetComponent<NetworkObject>().OwnerClientId);
        }
        if (IsOwnedByServer)
        {
            pm.GetComponent<OnlinePlayerManager>().SetMaxNumberOfPlayersRpc(GetComponent<OnlineSceneManager>().GetMaxNumberOfPlayers());
        }
    }
}
