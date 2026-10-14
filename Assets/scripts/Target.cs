using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Pathfinding;

public abstract class Target : MonoBehaviour
{
    public int maxHp;
    private int hp;
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
    [SerializeField] private int playerId;
    private bool infected = false;
    private GameObject deathExplosion;
    private int deathId;

    public void Start()
    {
        hp = maxHp;
        normalMaterial = GetComponent<SpriteRenderer>().material;
    }

    public int GetHp()
    {
        return hp;
    }

    public void SetHp(int value)
    {
        hp = value;
    }

    public void TakeDamage(int amount, bool penetration)
    {
        if (!invincible)
        {
            if (!penetration)
            {
                int blocked = amount * defense / 100;
                amount -= blocked;
            }
            hp -= amount;
            StartCoroutine(HurtState());
            Instantiate(bloodEffect, transform.position, Quaternion.identity);
            if (hp <= 0)
            {
                Die();
            }
            healthBar.ChangeSprites(maxHp, hp);
        }
    }

    public void TakeDamage(int amount, Unit unit, bool penetration)
    {
        Debug.Log("not done yet");
    }

    public void Die()
    {
        if (GetComponent<Hero>())
        {
            GameEvents.current.HeroDeath();           
            gameObject.SetActive(false);
            transform.position = new Vector2(1000f, 1000f);
        }
        else if (!isDead)
        {
            isDead = true;
            GameEvents.current.Death(gameObject.GetComponent<Target>());
            if (GetComponent<Unit>())
                Destroy(GetComponent<Unit>().dest.gameObject); //destroys destination for pathfinder
            if (GetComponent<Building>())
            {
                GetComponent<GraphUpdateScene>().Apply();
                if (GetComponent<Barracks>())
                {
                    GameEvents.current.CapacitySet(-2);
                }
            }
            if (GetComponent<BossPart>())
                GameEvents.current.BossPartDeath();
            else if (GetComponent<Boss>())
            {
                GetComponent<Boss>().Die();
            }
            if (GetComponent<Unit>())
                Instantiate(corpse, transform.position, Quaternion.identity);
            if (infected)
            {
                Instantiate(deathExplosion, transform.position, Quaternion.identity).GetComponent<SplashDamage>().Activate(100, deathId);
            }
            Destroy(gameObject);
        }
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

    public int GetId() => playerId;

    public void SetId(int value) => playerId = value;

    public void Infect(int value, GameObject explosion)
    {
        infected = true;
        deathExplosion = explosion;
        deathId = value;
    }
}
