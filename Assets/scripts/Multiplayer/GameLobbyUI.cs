using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobbyUI : MonoBehaviour
{
    [SerializeField] private GameLobbyItem lobbyTemplate;
    private List<GameLobbyItem> lobbyInstances = new List<GameLobbyItem>();
    [SerializeField] private GameLobby lobbyManager;

    public async void ShowLobbies()
    {
        try
        {
            if(lobbyInstances.Count > 0)
            {
                DeleteLobbyInstances();
            }
            float offset = 0;
            List<Lobby> lobbies = await lobbyManager.RefreshLobbyList();
            foreach (Lobby lobby in lobbies)
            {
                if(lobby.Players.Count != lobby.MaxPlayers)
                {
                    GameLobbyItem lobbyItem = Instantiate(lobbyTemplate, transform.position, Quaternion.identity, transform);

                    lobbyItem.GetComponent<RectTransform>().position = transform.position + new Vector3(0, offset, 0);
                    lobbyItem.gameObject.SetActive(true);
                    offset -= 0.5f;
                    lobbyItem.SetName(lobby.Name);
                    lobbyItem.SetPlayerCount(lobby.Players.Count, lobby.MaxPlayers);
                    lobbyItem.SetLobbyId(lobby.Id);

                    lobbyInstances.Add(lobbyItem);
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    private void DeleteLobbyInstances()
    {
        foreach(var lobby in lobbyInstances)
        {
            Destroy(lobby.gameObject);
        }
        lobbyInstances.Clear();
    }

    public void JoinLobbyById(string id)
    {
        lobbyManager.JoinLobbyById(id);
    }
}
