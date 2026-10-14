using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Unity.Netcode;

public class OnlineRepairUnit : MonoBehaviour
{
    private OnlineUnit unit;
    private OnlineBuilding repairingBuilding;
    private bool repairCooldown = false;
    private bool repairing = false;
    public AudioClip repairSound;
    private AudioClip normalSound;

    public void Start()
    {
        unit = GetComponent<OnlineUnit>();
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
            if(repairingBuilding.GetHp() < repairingBuilding.maxHp)
            {
                GetComponent<AudioSource>().clip = repairSound;
                GetComponent<AudioSource>().Play();
                GetComponent<Animator>().CrossFade("Repair", 0, 0);
                repairCooldown = true;

                if (GetComponent<OnlineUnit>().IsServer)
                {
                    repairingBuilding.SetHealthRpc(repairingBuilding.GetHp() + 5);
                }

                repairingBuilding.healthBar.ChangeSprites(repairingBuilding.maxHp, repairingBuilding.GetHp());
                if (repairingBuilding.maxHp < repairingBuilding.GetHp())
                {
                    repairingBuilding.SetHealthRpc(repairingBuilding.maxHp);
                    repairingBuilding = null;
                }
                StartCoroutine(Cooldown());
            }   
        }
    }

    public void Repair(OnlineBuilding target)
    {
        var distance = Vector2.Distance(target.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.position);
        /*if (distance > 0.8f)
        {
            unit.Move(Vector2.MoveTowards(transform.position, target.transform.position, distance - 0.5f));
            repairingBuilding = target;
        }
        else
        {
            unit.Move(transform.position);
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            repairingBuilding = target;
            repairing = true;
        }*/
        if (distance < 1.3f)
        {
            GetComponent<OnlineUnit>().PathDone();
            repairing = true;
        }
    }

    public void SetRepairingBuilding(OnlineBuilding building)
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
