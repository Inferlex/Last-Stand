using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src;
using Assets.Src.Units;

public class Boss : Unit
{
    private int currentParts = -1;
    private int phase = 0;
    public List<BossPart> parts;
    private bool revealed = false;
    public GameObject effect;
    private bool canAttack = false;
    private List<BossPart> bossParts;
    public GameObject deathObject;

    new private void Start()
    {
        bossParts = new List<BossPart>();
        healthBar.ChangeTransparency(false);
        base.Start();       
        GameEvents.current.OnBossPartDeath += OnBossPartDeath;
        CreateParts(2, 2);
        StartCoroutine(Disappear());
    }

    new private void Update()
    {
        if (currentParts == 0)
        {
            if (!revealed)
            {
                StartCoroutine(Appear());
                SetDefense(20);
                if (phase == 2)
                    StartCoroutine(BerserkMode());
                revealed = true;
                canAttack = true;
                if (phase != 2)
                    StartCoroutine(WaitToDisappear());
            }
        }
        if (!GetCooldown() && canAttack)
        {
            SetCooldown(true);
            Attack();
        }
        switch (phase)
        {
            case 0:
                if (GetHp() < 2000)
                {
                    CreateParts(3, 2);
                    StartCoroutine(Disappear());
                    phase++;
                }
                break;
            case 1:
                if (GetHp() < 1000)
                {
                    CreateParts(3, 3);
                    StartCoroutine(Disappear());
                    phase++;
                }
                break;
            case 2:
                break;
        }
    }

    private void Attack()
    {
        GetComponent<AudioSource>().Play();
        GameObject spl = Instantiate(splashObject, transform.position, Quaternion.identity);
        spl.GetComponent<SplashDamage>().Activate(damage, GetId());
        StartCoroutine(Wait());
    }

    private void OnBossPartDeath()
    {
        currentParts--;
    }

    private void CreateParts(int tCount, int eCount)
    {
        for (int i = 0; i < tCount; i++)
        {
            bossParts.Add(Instantiate(parts[0]));
        }
        for (int i = 0; i < tCount; i++)
        {
            bossParts.Add(Instantiate(parts[1]));
        }
        currentParts = tCount + eCount;
    }

    private IEnumerator Appear()
    {
        GetRandomPosition();
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 255);
        healthBar.ChangeTransparency(true);
        GetComponent<Animator>().CrossFade("Reveal", 0, 0);
        yield return new WaitForSeconds(1f);
        GetComponent<CircleCollider2D>().enabled = true;
    }

    private IEnumerator Disappear()
    {
        canAttack = false;
        SetDefense(100);
        GetComponent<Animator>().CrossFade("Disappear", 0, 0);
        yield return new WaitForSeconds(1f);
        revealed = false;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 0);
        healthBar.ChangeTransparency(false);
    }

    private IEnumerator WaitToDisappear()
    {
        for(int i = 0; i< 20; i++) //cycle is used for when this function is called more times at the same time
        {
            yield return new WaitForSeconds(1f);
            if (!canAttack)
            {
                break;
            }
        }
        if (canAttack)
        {
            CreateParts(1, 1);
            StartCoroutine(Disappear());
        }
    }

    private IEnumerator BerserkMode()
    {
        SetDefense(75);
        effect.SetActive(true);
        while (!GetIsDead())
        {
            yield return new WaitForSeconds(20f);
            CreateParts(2, 1);
        }
    }

    public void GetRandomPosition()
    {
        Player pl = FindObjectOfType<Player>();
        int index;
        if (Random.Range(0, 2) == 0)
        {
            bool canSpawn = false;
            while (!canSpawn)
            {
                if (pl.buildings.Count > 0)
                {
                    index = GenerateRandomInt(0, pl.buildings.Count);
                    Transform tr = pl.buildings[index].transform;
                    transform.position = new Vector3(tr.position.x + Random.Range(0, 3), tr.position.y + Random.Range(0, 3), -1);
                }
                if (!Physics2D.OverlapPoint(transform.position).CompareTag("PlayerBuilding"))
                    canSpawn = true;
            }
        }
        else
        {
            if (pl.units.Count > 0)
            {
                index = GenerateRandomInt(0, pl.units.Count);
                Transform tr = pl.units[index].transform;
                transform.position = new Vector3(tr.position.x + Random.Range(0, 3), tr.position.y + Random.Range(0, 3), -1);
            }
        }
    }

    private int GenerateRandomInt(int min, int max)
    {
        int rand = Random.Range(min, max);
        return rand;
    }

    private float GenerateRandomFloat(float min, float max)
    {
        float rand = Random.Range(min, max);
        return rand;
    }

    public void DestroyAllParts()
    {
        foreach(BossPart part in bossParts)
        {
            if(part)
                part.Die();
        }
    }

    new public void Die()
    {
        DestroyAllParts();
        Instantiate(deathObject, transform.position, Quaternion.identity);
    }
}
