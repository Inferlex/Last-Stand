using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameLobbyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI PlayerCountText;
    private string lobbyId;

    public void SetName(string s)
    {
        NameText.text = s;
    }

    public void SetPlayerCount(int count, int maxCount)
    {
        PlayerCountText.text = $"{count}/{maxCount}";
    }

    public void SetLobbyId(string s)
    {
        lobbyId = s;
    }

    public string GetLobbyId()
    {
        return lobbyId;
    }

    public void JoinLobby()
    {
        Debug.Log(transform.parent);
        transform.parent.GetComponent<GameLobbyUI>().JoinLobbyById(lobbyId);
    }
}
