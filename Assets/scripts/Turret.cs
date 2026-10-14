using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class Turret : Building
{

    Enemy selectedTarget;
    private bool isFiring = false;
    public float fireRate;
    public int damage;
    public CircleCollider2D radius;
    public bool isSummoned;
    private List<Enemy> enemiesInRange = new List<Enemy>();

    new void Start()
    {
        base.Start();
        if (isSummoned)
            StartCoroutine(StartDying());
    }

    void Update()
    {
        if (!isFiring && selectedTarget)
        {
            if (transform.position.x > selectedTarget.transform.position.x)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (transform.position.x < selectedTarget.transform.position.x)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            GetComponent<Animator>().CrossFade("Attack", 0, 0);
            GetComponent<AudioSource>().Play();
            selectedTarget.TakeDamage(damage, false);
            StartCoroutine(Cooldown());
        }
        if(selectedTarget == null && enemiesInRange.Count > 0)
        {
            selectedTarget = enemiesInRange[0];
            enemiesInRange.Remove(enemiesInRange[0]);
        }
    }

    public IEnumerator Cooldown()
    {
        isFiring = true;
        yield return new WaitForSeconds(fireRate);
        isFiring = false;       
    }
    
    public void TargetEnter(Enemy enemy)
    {
        if (selectedTarget == null)
        {
            selectedTarget = enemy;
        }
        else
        {
            enemiesInRange.Add(enemy);
        }
    }

    public void TargetExit(Enemy enemy)
    {
        if (enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }
    }

    private IEnumerator StartDying()
    {
        SetHp(GetHp() - 30);
        if (GetHp() <= 0)
            Die();
        yield return new WaitForSeconds(1f);
        StartCoroutine(StartDying());
    }
}
