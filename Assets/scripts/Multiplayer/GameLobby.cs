using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{

    private Lobby hostLobby;
    private float heartbeatTimer;
    private float updateTimer;
    private Lobby joinedLobby;
    private bool isPrivateLobby = false;
    private int maxPlayers = 2;
    private string lobbyCode;
    private string lobbyName;

    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private TextMeshProUGUI joinText;
    [SerializeField] private TextMeshProUGUI PlayersText;
    [SerializeField] private TextMeshProUGUI PlayerNamesText;
    [SerializeField] private GameObject startButton;

    [SerializeField] private GameRelay relay;

    [SerializeField] private GameObject LobbyList;

    private string playerName;

    //private bool hostingLobby = false;
    //private bool joiningLobby = false;
    public const string KEY_START_GAME = "0";

    async void Start() //when playing true multiplayer
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("signed " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }    
        playerName = "Anonymous";
        lobbyName = "my lobby";
        PlayerNamesText.gameObject.SetActive(false);
    }

    public async void CreateLobby()
    {
        try
        {
            PlayersText.text = $"1/{maxPlayers} players";
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivateLobby,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;
            joinedLobby = lobby;
            PlayerNamesText.gameObject.SetActive(true);

            
            Debug.Log($"created lobby {lobby.Name} id {lobby.Id} and code {lobby.LobbyCode} max players {maxPlayers} is private {lobby.IsPrivate}");
            codeText.text = $"Lobby code is: {hostLobby.LobbyCode}";
            //PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("lobbies found " + queryResponse.Results);
            foreach (var lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            try
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer < 0f)
                {
                    float heartbeatTimerMax = 15f;
                    heartbeatTimer = heartbeatTimerMax;

                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private async void HandleLobbyForUpdates()
    {
        try
        {
            if (joinedLobby != null)
            {
                updateTimer -= Time.deltaTime;
                if (updateTimer < 0f)
                {
                    float updateTimerMax = 1.1f;
                    updateTimer = updateTimerMax;

                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;

                    if (hostLobby != null)
                    {
                        PlayersText.text = $"{lobby.Players.Count}/{maxPlayers} players";
                        if(lobby.Players.Count > 1)
                        {
                            startButton.SetActive(true);
                        }
                        else
                        {
                            startButton.SetActive(false);
                        }
                    }

                    ShowPlayers();

                    if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                    {
                        if(hostLobby == null)
                        {
                            await relay.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                        }

                        joinedLobby = null;
                    }
                }

            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            PlayerNamesText.text = "";
            joinedLobby = null;
            joinText.text = "Lobby no longer exists";
        }
    }

    private void ShowPlayers()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var player in joinedLobby.Players)
        {
            sb.Append($"{player.Data["PlayerName"].Value}\n");
        }
        PlayerNamesText.text = sb.ToString();
    }

    public async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("lobbies found " + queryResponse.Results.Count);

            if (queryResponse.Results.Count > 0)
            {
                JoinLobbyByIdOptions joinLobbyOptions = new JoinLobbyByIdOptions
                {
                    Player = GetPlayer()
                };
                Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id, joinLobbyOptions);
                joinedLobby = lobby;
                joinText.text = "Lobby found!";
                PlayerNamesText.gameObject.SetActive(true);
            }
            else
            {
                joinText.text = "Lobby not found!";
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };


            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            joinedLobby = lobby;
            joinText.text = "Lobby found!";
            PlayerNamesText.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            joinText.text = "Lobby not found!";
        }
    }

    public async void JoinLobbyById(string id)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, options);
            joinedLobby = lobby;
            joinText.text = "Lobby found!";
            PlayerNamesText.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            joinText.text = "Lobby not found!";
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            PlayerNamesText.gameObject.SetActive(false);
            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            PlayerNamesText.text = "";
            joinText.text = "searching for lobby";     
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            PlayerNamesText.gameObject.SetActive(false);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            PlayerNamesText.gameObject.SetActive(false);
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            PlayerNamesText.text = "";
            startButton.SetActive(false);           
        }
        catch (LobbyServiceException e)
        {
            PlayerNamesText.gameObject.SetActive(false);
            Debug.Log(e);
        }
    }

    public async Task<List<Lobby>> RefreshLobbyList()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            return queryResponse.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public void SetPrivate(bool val)
    {
        isPrivateLobby = val;
    }

    public void SetMaxPlayers(int i)
    {
        maxPlayers = i + 2;
    }

    public void SetCode(string s)
    {
        lobbyCode = s;
    }

    public void SetPlayerName(string s)
    {
        playerName = s;
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject( PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
    }

    public async void StartGame()
    {
        if (hostLobby != null && joinedLobby.Players.Count > 1)
        {
            try
            {
                Debug.Log("starting game");
                string relayCode = await relay.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        else
        {
            Debug.Log("not enough players");
        }
    }

    public int GetMaxPlayers() => maxPlayers;

    public void SetLobbyName(string s)
    {
        lobbyName = s;
    }
}
