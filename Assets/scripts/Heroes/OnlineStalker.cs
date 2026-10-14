using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineStalker : OnlineHero
{
    public GameObject[] abilities;
    [SerializeField] private GameObject[] preAbilities;
    [SerializeField] private GameObject cloneExplosion;
    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        //GameEvents.current.OnTargetAbility += TargetAbility;
        if (IsOwner)
        {
            powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Corrosive Puddle";
            SetPowerDescription(0, "[Q] Creates a puddle that damages nearby enemies.");
            powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Killer Instinct";
            SetPowerDescription(1, "[W] Temporalily increases speed of the Stalker.");
            powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Chaos";
            SetPowerDescription(2, "[E] Creates three copies of itself. Copies are weaker and explode on death");
        }
    }

    public override void UseAbility(int index)
    {
        if (!IsOnCooldown(index))
        {
            switch (index)
            {
                case 0:
                    Ability1();
                    break;
                case 1:
                    Ability2();
                    StartCoroutine(Cooldown(1, abilityCooldown[1]));
                    break;
                case 2:
                    Ability3();
                    StartCoroutine(Cooldown(2, abilityCooldown[2]));
                    break;
            }
        }
    }

    public override void LevelUpStats()
    {
        switch (GetLevel())
        {
            case 2:
                damage += 10;
                maxHp += 80;
                break;
            case 3:
                fireRate -= 0.1f;
                maxHp += 80;
                //abilityCooldown[0] -= 6f;
                break;
            case 4:
                damage += 12;
                maxHp += 100;
                //abilityCooldown[1] -= 6;
                break;
            case 5:
                damage += 12;
                fireRate -= 0.1f;
                maxHp += 120;
                //abilityCooldown[0] -= 4f;
                break;
            case 6:
                damage += 14;
                fireRate -= 0.1f;
                maxHp += 120;
                break;
        }
        SetHealthRpc(maxHp);
    }

    private new void Ability1()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[0]).GetComponent<PreAbility>().SetHero(this);
    }

    private new void Ability2()
    {
        Ability2Rpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void Ability2Rpc()
    {
        StartCoroutine(KillerInstinct());
    }

    private new void Ability3()
    {
        Ability3Rpc();
    }

    [Rpc(SendTo.Server)]
    private void Ability3Rpc()
    {
        for (int i = 0; i < 3; i++)
        {
            OnlineUnit unit = Instantiate(abilities[1], transform.position, Quaternion.identity).GetComponent<OnlineUnit>();
            unit.Infect(GetId(), cloneExplosion);
            unit.GetComponent<NetworkObject>().Spawn();
            unit.GetComponent<OnlineTarget>().SetNetworkIdRpc(GetId());
        }
    }

    /*private IEnumerator Invisible() //too much of a pain to implement
    {
        if (GetIsOwnedByPlayer())
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 100);
        else
            Vanish();

        if (!GetIsOwnedByPlayer())
            GetComponent<CapsuleCollider2D>().enabled = false;

        yield return new WaitForSeconds(15f);

        

        if (!GetIsOwnedByPlayer())
        {
            GetComponent<CapsuleCollider2D>().enabled = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        }
    }*/

    private IEnumerator KillerInstinct()
    {
        fireRate -= 0.2f;
        GetComponent<UnitAI>().speed += 250;
        yield return new WaitForSeconds(15f);
        fireRate += 0.2f;
        GetComponent<UnitAI>().speed -= 250;
    }

    public override void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 5))
        {
            int abilityId = ability.GetComponent<PreAbility>().index;
            TargetAbilityRpc(ability.transform.position.x, ability.transform.position.y, abilityId);
            StartCoroutine(Cooldown(abilityId, abilityCooldown[abilityId]));
            SetAbilityRadiusVisibility(false);
        }
        else
        {
            GameEvents.current.ShowMessage("Ability is not within range/" + GetId());
            Destroy(ability); //ability neni online, takze nemusime volat server
            SetAbilityRadiusVisibility(false);
        }
    }

    [Rpc(SendTo.Server)]
    private void TargetAbilityRpc(float x, float y, int id)
    {
        GameObject ability = Instantiate(abilities[id], new Vector3(x, y, -1f), transform.rotation);
        ability.GetComponent<NetworkObject>().Spawn(true);
        ability.GetComponent<OnlineAbility>().SetId(GetId());
        ability.GetComponent<OnlineAbility>().Activate(this);
    }

    public override void SetButtons()
    {
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Corrosive Puddle";
        SetPowerDescription(0, "[Q] Creates a puddle that damages nearby enemies.");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Killer Instinct";
        SetPowerDescription(1, "[W] Temporalily increases speed of the Stalker.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Chaos";
        SetPowerDescription(2, "[E] Creates three copies of itself. Copies are weaker and explode on death");
    }
}
