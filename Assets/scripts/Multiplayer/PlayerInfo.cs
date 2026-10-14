using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private int heroId;
    private int factionId;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetHero(int id) => this.heroId = id;

    public int GetHero() => heroId;

    public void SetFaction(int id) => this.factionId = id;

    public int GetFaction() => factionId;
}
