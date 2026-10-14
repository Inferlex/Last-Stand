using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Pathfinding;
using Unity.Netcode;
using Unity.VisualScripting;

public abstract class OnlineTarget : NetworkBehaviour
{
    public int maxHp;
    //private int hp; //replaced with networkHealth
    public int points;
    public string targetName;
    public GameObject bloodEffect;
    private bool isDead = false;
    public int defense;
    public Material hurtMaterial;
    private Material normalMaterial;
    private bool invincible = false;
    public GameObject corpse;
    public HealthBar healthBar;
    //[SerializeField] private int playerId;
    private bool infected = false;
    private GameObject deathExplosion;
    private int deathId = 999;

    private NetworkVariable<int> networkId = new NetworkVariable<int>(999);
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>();

    private SpriteRenderer shadowSprite;
    private SpriteRenderer minimapSprite;

    private int unitsLooking = 0;
    private bool isOwnedByPlayer = true;
    private int localId = 999;
    private CircleCollider2D visionRadius;

    [SerializeField] private int listId = 999;
    //private int uniqueID = 999;

    public override void OnNetworkSpawn()
    {
        networkId.OnValueChanged += OnIdChanged;
        networkHealth.OnValueChanged += OnHealthChanged;
    }

    public void OnIdChanged(int prev, int current)
    {
        networkId.Value = current;
        ChangeColor();

        if (visionRadius == null)
            visionRadius = gameObject.transform.GetChild(0).GetComponent<CircleCollider2D>();

        if (CompareTag("PlayerUnit"))
        {
            GameEvents.current.UnitCreation(GetComponent<OnlineUnit>());
            if (GetComponent<CapsuleCollider2D>())
            {
                GetComponent<CapsuleCollider2D>().enabled = true;
            }
            else
            {
                GetComponent<CircleCollider2D>().enabled = true;
            }
        }
        else if (CompareTag("PlayerBuilding"))
        {
            GameEvents.current.BuildCreation(GetComponent<OnlineBuilding>());
            GetComponent<BoxCollider2D>().enabled = true;
        }

        GameEvents.current.PointsEarn(new int[2] { points, GetId() });
        localId = current;
        if (isOwnedByPlayer)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            healthBar.GetComponent<SpriteRenderer>().color = Color.white;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == "Revealer")
                {
                    transform.GetChild(i).gameObject.GetComponent<SpriteMask>().enabled = true;
                    break;
                }
            }
        }
    }

    public void OnHealthChanged(int prev, int current)
    {
        networkHealth.Value = current;
        healthBar.ChangeSprites(maxHp, networkHealth.Value);
    }

    [Rpc(SendTo.Server)]
    public void SetNetworkIdRpc(int val)
    {
        networkId.Value = val;
    }

    [Rpc(SendTo.Server)]
    public void SetHealthRpc(int val)
    {
        networkHealth.Value = val;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        if (GetId() == 999)
        {
            GetComponent<SpriteRenderer>().color = Color.clear;
            healthBar.GetComponent<SpriteRenderer>().color = Color.clear;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == "Revealer")
                {
                    transform.GetChild(i).gameObject.GetComponent<SpriteMask>().enabled = false;
                    break;
                }
            }
        }
        SetHealthRpc(maxHp);
        normalMaterial = GetComponent<SpriteRenderer>().material;
        visionRadius = gameObject.transform.GetChild(0).GetComponent<CircleCollider2D>();
    }

    public int GetHp()
    {
        return networkHealth.Value;
    }

    public void TakeDamage(int amount, OnlineBuilding unit, bool penetration) //ok, its used by turrets
    {
        if (!IsServer) return;
        if (!invincible)
        {
            if (!penetration)
            {
                int blocked = amount * defense / 100;
                amount -= blocked;
            }

            if (IsServer)
                SetHealthRpc(GetHp() - amount);

            HurtStateRpc();

            if (GetHp() <= 0)
            {
                SendDeathEventRpc();
                GameEvents.current.PointsEarn(new int[2] { points, unit.GetId() });
                DieRpc();
            }
            else
            {
                healthBar.ChangeSprites(maxHp, GetHp());
            }

        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void HurtStateRpc()
    {
        Instantiate(bloodEffect, transform.position, Quaternion.identity);
        StartCoroutine(HurtState());
    }

    public void TakeDamage(int amount, OnlineUnit unit, bool penetration)
    {
        if (!IsServer) return;
        if (!invincible)
        {
            if (!penetration) //if the damage is not penetrative, defense will lessen the damage
            {
                int blocked = amount * defense / 100;
                amount -= blocked;
            }

            SetHealthRpc(GetHp() - amount);

            HurtStateRpc();

            /*int attackDamage = 0;
            if (GetId() == 0)
            {
                attackDamage = amount;
            }*/

            if (GetHp() <= 0)
            {
                if (unit.GetComponent<OnlineHero>() && GetComponent<OnlineUnit>()) //if a hero kill the enemy, give him xp
                {
                    unit.GetComponent<OnlineHero>().GetXPRpc(GetComponent<OnlineUnit>().xp);
                }
                SendDeathEventRpc();
                GameEvents.current.PointsEarn(new int[2] { points, unit.GetId() });
                DieRpc();
            }
            healthBar.ChangeSprites(maxHp, GetHp());
        }
    }

    public void TakeDamage(int amount, bool penetration)
    {
        if (!invincible)
        {
            if (!penetration) //if the damage is not penetrative, defense will lessen the damage
            {
                int blocked = amount * defense / 100;
                amount -= blocked;
            }
            SetHealthRpc(GetHp() - amount);
            //Instantiate(bloodEffect, transform.position, Quaternion.identity);
            HurtStateRpc();

            int attackDamage = 0;
            if (GetId() == 0)
            {
                attackDamage = amount;
            }

            if (GetHp() - attackDamage <= 0)
            {
                SendDeathEventRpc();
                DieRpc();
            }
            healthBar.ChangeSprites(maxHp, GetHp());
        }
    }

    [Rpc(SendTo.Server)]
    public void DieRpc()
    {
        if (GetComponent<OnlineHero>())
        {
            HeroDeathRpc();
            return;
        }
        isDead = true;
        if (GetComponent<OnlineBuilding>())
        {
            GetComponent<GraphUpdateScene>().Apply(); // I dont think this does anything
        }
        if (GetComponent<OnlineUnit>())
        {
            Destroy(GetComponent<OnlineUnit>().dest.gameObject);
            SpawnCorpseRpc();
        }

        if (infected)
        {
            //Instantiate(deathExplosion, transform.position, Quaternion.identity).GetComponent<SplashDamage>().Activate(100, deathId);
            Explode();
        }
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnCorpseRpc()
    {
        Instantiate(corpse, transform.position, Quaternion.identity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void HeroDeathRpc()
    {
        GameEvents.current.OnlineHeroDeath(GetId());
        gameObject.SetActive(false);
        transform.position = new Vector2(1000f, 1000f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendDeathEventRpc()
    {
        GameEvents.current.Death(GetComponent<OnlineTarget>());
    }

    private void Explode()
    {
        GameObject obj = Instantiate(deathExplosion, transform.position, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();
        obj.GetComponent<OnlineSplashDamage>().Activate(80, deathId);
    }

    public IEnumerator HurtState()
    {
        GetComponent<SpriteRenderer>().material = hurtMaterial;
        yield return new WaitForSeconds(0.08f);
        GetComponent<SpriteRenderer>().material = normalMaterial;
    }

    public void SetInvincible(bool value)
    {
        invincible = value;
    }

    public void ResetMaterial()
    {
        GetComponent<SpriteRenderer>().material = normalMaterial;
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    public void SetDefense(int value)
    {
        defense = value;
    }

    public int GetId() => networkId.Value;

    public void Infect(int value, GameObject explosion)
    {
        infected = true;
        deathExplosion = explosion;
        deathId = value;
    }

    public void ChangeColor()
    {
        bool hasShadow = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
            {
                if (!hasShadow)
                {
                    hasShadow = true;
                    shadowSprite = transform.GetChild(i).GetComponent<SpriteRenderer>();
                }
                else
                {
                    minimapSprite = transform.GetChild(i).GetComponent<SpriteRenderer>();
                    break;
                }
            }
        }

        switch (GetId())
        {
            case 0:
                shadowSprite.color = new Color32(150, 0, 0, 100);
                minimapSprite.color = Color.red;
                break;
            case 1:
                shadowSprite.color = new Color32(0, 0, 150, 100);
                minimapSprite.color = Color.blue;
                break;
            case 2:
                shadowSprite.color = new Color32(0, 150, 0, 100);
                minimapSprite.color = Color.green;
                break;
            case 3:
                shadowSprite.color = new Color32(150, 150, 0, 100);
                minimapSprite.color = Color.yellow;
                break;
        }
    }

    public void ChangeShadow()
    {
        if (shadowSprite != null)
            switch (GetId())
            {
                case 0:
                    shadowSprite.color = new Color32(150, 0, 0, 100);
                    break;
                case 1:
                    shadowSprite.color = new Color32(0, 0, 150, 100);
                    break;
                case 2:
                    shadowSprite.color = new Color32(0, 150, 0, 100);
                    break;
                case 3:
                    shadowSprite.color = new Color32(150, 150, 0, 100);
                    break;
            }
    }

    public void ChangeMinimapSprite()
    {
        switch (GetId())
        {
            case 0:
                minimapSprite.color = Color.red;
                break;
            case 1:
                minimapSprite.color = Color.blue;
                break;
            case 2:
                minimapSprite.color = Color.green;
                break;
            case 3:
                minimapSprite.color = Color.yellow;
                break;
        }
    }

    public void DeactivateMask()
    {
        shadowSprite.enabled = false;
        minimapSprite.enabled = false;
        healthBar.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<SpriteMask>())
            {
                transform.GetChild(i).gameObject.SetActive(false);
                break;
            }
        }
        isOwnedByPlayer = false;
    }

    public void IncreaseUnitsLooking()
    {
        if (!isOwnedByPlayer)
        {
            unitsLooking++;
            if (unitsLooking >= 1)
            {
                Reveal();
            }
        }
    }

    public void DecreaseUnitsLooking()
    {
        if (!isOwnedByPlayer)
        {   
            unitsLooking--;
            if(unitsLooking < 0)
            {
                unitsLooking = 0;
            }
            if (unitsLooking == 0)
            {
                Vanish();
            }
        }
    }

    public int GetUnitsLooking()
    {
        return unitsLooking;
    }

    public void FixVanish()
    {
        unitsLooking = 0;
        Vanish();
    }

    private void Reveal()
    {
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        shadowSprite.enabled = true;
        minimapSprite.enabled = true;
        healthBar.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void Vanish()
    {
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        shadowSprite.enabled = false;
        minimapSprite.enabled = false;
        healthBar.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
    }

    public bool GetIsOwnedByPlayer()
    {
        return isOwnedByPlayer;
    }

    public int GetLocalId() => localId;

    [Rpc(SendTo.Everyone)]
    public void WeakenRpc() => StartCoroutine(Debuff());

    private IEnumerator Debuff()
    {
        if (unitsLooking > 0 || IsOwner)
            GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 255);
        GetComponent<OnlineUnit>().damage /= 2;
        GetComponent<UnitAI>().speed /= 2;

        yield return new WaitForSeconds(5f);

        GetComponent<OnlineUnit>().damage *= 2;
        GetComponent<UnitAI>().speed *= 2;

        if (unitsLooking > 0 || IsOwner)
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }

    public CircleCollider2D GetVisionRadius()
    {
        return visionRadius;
    }

    public void SetListId(int index)
    {
        listId = index;
    }

    public int GetListId() { return listId; }

    /*public void SetUniqueID(int index) => uniqueID = index;

    public int GetUniqueID() { return uniqueID; }*/
}
