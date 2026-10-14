using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkWait : MonoBehaviour
{
    private int players;
    [SerializeField] private TextMeshProUGUI serverText;
    [SerializeField] private TextMeshProUGUI clientText;

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        NetworkManager.Singleton.OnConnectionEvent += FoundConnection;
    }

    private void ClientConnected(ulong id)
    {
        players++;
        serverText.text = $"{players}/{GetComponent<MainMenu>().GetMaxNumberOfPlayers()} players";
    }

    private void FoundConnection(NetworkManager manager, ConnectionEventData data)
    {
        if (clientText && clientText.gameObject.activeInHierarchy)
        {
            clientText.text = "Connected to Lobby. Waiting for other players";
        }
    }
}
