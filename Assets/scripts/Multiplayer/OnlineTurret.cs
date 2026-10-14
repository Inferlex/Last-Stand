using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Unity.Netcode;

public class OnlineTurret : OnlineBuilding
{
    OnlineUnit selectedTarget;
    private bool isFiring = false;
    public float fireRate;
    public int damage;
    public CircleCollider2D radius;
    public bool isSummoned;
    private List<OnlineUnit> enemiesInRange = new List<OnlineUnit>();

    new void Start()
    {
        base.Start();
        if (isSummoned)
            StartDyingRpc();
    }

    void Update()
    {
        if (IsActivated())
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
                if (IsServer) //so it only does the attack on server
                    selectedTarget.TakeDamage(damage, this, false);
                StartCoroutine(Cooldown());
            }
            if (selectedTarget == null && enemiesInRange.Count > 0)
            {
                selectedTarget = enemiesInRange[0];
                enemiesInRange.Remove(enemiesInRange[0]);
            }
        }
    }

    public IEnumerator Cooldown()
    {
        isFiring = true;
        yield return new WaitForSeconds(fireRate);
        isFiring = false;       
    }
    
    public void TargetEnter(OnlineUnit enemy)
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

    public void TargetExit(OnlineUnit enemy)
    {
        if (enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }
        if(enemy == selectedTarget)
        {
            if(enemiesInRange.Count > 0)
            {
                selectedTarget = enemiesInRange[0];
            }
            else
            {
                selectedTarget = null;
            }
        }
    }

    [Rpc(SendTo.Server)] 
    private void StartDyingRpc()
    {
        if (isSummoned)
        {
            isSummoned = false; //so it wont be called more times
            StartCoroutine(StartDying());
        }
    }

    private IEnumerator StartDying()
    {
        SetHealthRpc(GetHp() - 30);
        if (GetHp() <= 0)
            DieRpc();
        yield return new WaitForSeconds(1f);
        StartCoroutine(StartDying());
    }
}
