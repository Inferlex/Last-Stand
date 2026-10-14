using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class RepairUnit : MonoBehaviour
{
    private Unit unit;
    private Building repairingBuilding;
    private bool repairCooldown = false;
    private bool repairing = false;
    public AudioClip repairSound;
    private AudioClip normalSound;

    public void Start()
    {
        unit = GetComponent<Unit>();
        normalSound = GetComponent<AudioSource>().clip;
    }

    public void Update()
    {
        if (!repairingBuilding) //the building is destryed
        {
            repairing = false;
        }
        if (repairingBuilding && !repairing) //building is assigned and is not reparing
        {
            Repair(repairingBuilding);
        }
        if (repairing && !repairCooldown) //is repairing and not on cooldown
        {
            GetComponent<AudioSource>().clip = repairSound;
            GetComponent<AudioSource>().Play();
            GetComponent<Animator>().CrossFade("Repair", 0, 0);
            repairCooldown = true;
            repairingBuilding.SetHp(repairingBuilding.GetHp() + 5);
            repairingBuilding.healthBar.ChangeSprites(repairingBuilding.maxHp, repairingBuilding.GetHp());
            if (repairingBuilding.maxHp < repairingBuilding.GetHp())
            {
                repairingBuilding.SetHp(repairingBuilding.maxHp);
                repairingBuilding = null;
            }            
            StartCoroutine(Cooldown());
        }
    }

    public void Repair(Building target)
    {
        var distance = Vector2.Distance(target.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.position);
        if (distance < 1.2f)
        {
            GetComponent<Unit>().PathDone();
            repairing = true;
        }
    }

    public void SetRepairingBuilding(Building building)
    {
        repairingBuilding = building;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1f);
        GetComponent<AudioSource>().clip = normalSound;
        repairCooldown = false;
    }
}
