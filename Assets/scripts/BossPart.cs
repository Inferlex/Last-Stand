using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Assets.Src;

public class BossPart : Enemy
{
    private List<Target> targetsInRange = new List<Target>();
    private int attackLeft;
    private bool isBurrowed = true;
    private CircleCollider2D col;

    new void Start()
    {
        //GameEvents.current.OnGameFinished += OnGameFinished;
        healthBar.ChangeTransparency(false);
        col = transform.GetChild(0).GetComponent<CircleCollider2D>();
        base.Start();
        GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(WaitToAppear());
    }

    new void Update()
    {
        if (!isBurrowed)
        {
            if (!GetCooldown())
            {
                if (attackLeft > 0)
                {
                    GetRandomTarget();
                    if (GetLockedUnit() != null)
                    {
                        GetComponent<AudioSource>().Play();
                        SetCooldown(true);
                        GetComponent<Animator>().CrossFade("Attack", 0, 0);
                        GameObject splash = Instantiate(splashObject, GetLockedUnit().transform.position, Quaternion.identity);
                        splash.GetComponent<SplashDamage>().Activate(damage, GetId());
                        StartCoroutine(Wait());
                        attackLeft--;
                    }
                }
                else
                {
                    isBurrowed = true;
                    StartCoroutine(Disappear());
                }
            }
        }
    }

    private void GetRandomTarget()
    {
        if (targetsInRange.Count > 0)
        {
            int index = GenerateRandomInt(0, targetsInRange.Count);
            SetLockedUnit(targetsInRange[index]);
        }
    }

    public void TargetEnter(Target target)
    {
        targetsInRange.Add(target);
    }

    public void TargetExit(Target target)
    {
        if (targetsInRange.Contains(target))
        {
            targetsInRange.Remove(target);
        }
        if(GetLockedUnit() == target)
        {
            SetLockedUnit(null);
        }
    }

    public IEnumerator Appear()
    {
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 255);
        healthBar.ChangeTransparency(true);
        GetComponent<Animator>().CrossFade("Reveal", 0, 0);       
        yield return new WaitForSeconds(1f);
        GetComponent<CircleCollider2D>().enabled = true;
        col.gameObject.SetActive(true);
        col.transform.position = transform.position;
        SetCooldown(false);
        attackLeft = Random.Range(2, 5);
        isBurrowed = false;
    }

    public IEnumerator Disappear()
    {
        GetComponent<Animator>().CrossFade("Disappear", 0, 0);
        yield return new WaitForSeconds(1f);
        GetComponent<CircleCollider2D>().enabled = false;
        col.gameObject.SetActive(false);
        //GetComponent<Collider2D>().gameObject.SetActive(false);
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 0);
        healthBar.ChangeTransparency(false);
        SetLockedUnit(null);
        targetsInRange.Clear();
        StartCoroutine(WaitToAppear());
    }

    public IEnumerator WaitToAppear()
    {
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 0);
        yield return new WaitForSeconds(GenerateRandomFloat(4f, 8f));
        GetRandomPosition();
        StartCoroutine(Appear());
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

    new public void Reveal() //makes enemy visible
    {
        if (GetUnitsLookingCount() > 0 && !isBurrowed)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
            gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 255);
        }
    }

    new public void Vanish() //makes enemy invisible
    {
        if (GetUnitsLookingCount() == 0 || isBurrowed)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
            gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
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

    /*public void OnGameFinished(bool value)
    {
        if (gameObject != null && value) //if the boss is dead, destroy his parts
        {
            TakeDamage(1000, true);
        }
    }*/
}
