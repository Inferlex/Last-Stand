using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class OnlineGameManager : NetworkBehaviour
{
    private int players = 0;
    [SerializeField]private OnlineHero[] heroes = new OnlineHero[6];
    [SerializeField]private OnlineBase[] bases = new OnlineBase[2];
    //private bool started = false;
    private int[] playersId = new int[4];
    private int times = 0;
    private OnlineHero currentHero;
    private int maxPlayers;
    private int playersPlaying = 0;
    private List<Vector2> startPosition = new List<Vector2>();

    private void Start()
    {
        startPosition.Add(new Vector2(-27f, 27f));
        startPosition.Add(new Vector2(27f, -27f));
        startPosition.Add(new Vector2(-27f, -27f));
        startPosition.Add(new Vector2(27f, 27f));
        playersId = Enumerable.Repeat(999, 4).ToArray();
    }

    public void SetPlayer(int hId, int id, int bId)
    {
        if (IsHost && players > 0) //host will only call this method once, that is to fix hero selection
            return;

        players++;

        
        StartGameRpc(hId, bId-2, id); //the -2 is there because we use bId to get the online manager, which is 2 and 3 (normal manager is 0 and 1)
    }

    [Rpc(SendTo.Server)]
    private void StartGameRpc(int hId, int bId, int id)
    {
        if (times >= maxPlayers || playersId.Contains(id)) return;
        playersId[times] = id;
        OnlineHero hero = Instantiate(heroes[hId], startPosition[times] + new Vector2(0f, -2f), Quaternion.identity);
        hero.GetComponent<NetworkObject>().Spawn(true);
        hero.SetNetworkIdRpc(id);
        currentHero = hero;

        OnlineBase playerBase = Instantiate(bases[bId], startPosition[times], Quaternion.identity);
        playerBase.GetComponent<NetworkObject>().Spawn(true);
        playerBase.SetNetworkIdRpc(id);
        times++;
    }

    public void SetMaxPlayers(int i)
    {
        maxPlayers = i;
    }

    public void IncrementPlayersPlaying()
    {
        playersPlaying++;
    }

    public void PlayerDied()
    {
        playersPlaying--;
        if (playersPlaying == 1)
        {
            GameEvents.current.GameFinished(true);
        }
    }
}
