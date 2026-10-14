using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineAcolyte : OnlineHero
{
    public GameObject fireEffect;
    public GameObject[] abilities;
    public Transform attackPosition;
    [SerializeField] private GameObject[] preAbilities;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        //fireEffect.transform.SetParent(null); Maybe add it later, it does shit now
        if (IsOwner)
        {
            powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Electric Orb";
            SetPowerDescription(0, "[Q] Creates an orb that will damage nearby enemies over time");
            powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Freeze Blast";
            SetPowerDescription(1, "[W] Damages and freezes enemies in a set area.");
            powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Steel Call";
            SetPowerDescription(2, "[E] Summon a Steel knight, an long-forgotten machine, for a short while.");
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
                damage += 8;
                maxHp += 30;
                break;
            case 3:
                damage += 10;
                maxHp += 30;
                abilityCooldown[0] -= 5f;
                break;
            case 4:
                damage += 10;
                maxHp += 50;
                abilityCooldown[1] -= 6f;
                break;
            case 5:
                damage += 12;
                fireRate -= 0.1f;
                maxHp += 80;
                abilityCooldown[1] -= 6f;
                break;
            case 6:
                damage += 15;
                fireRate -= 0.2f;
                maxHp += 80;
                break;
        }
        SetHealthRpc(maxHp);
    }

    public void ShowAttack(OnlineTarget target)
    {
        GameObject effect = Instantiate(fireEffect);
        effect.GetComponent<LineController>().SetPoints(attackPosition.transform, target.transform);
    }

    private new void Ability1()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[0], transform.position, Quaternion.identity).GetComponent<PreAbility>().SetHero(this);
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[1], transform.position, Quaternion.identity).GetComponent<PreAbility>().SetHero(this);
    }
    
    private new void Ability3()
    {
        //Instantiate(abilities[2], transform.position, Quaternion.identity);
        Ability3Rpc();
    }

    [Rpc(SendTo.Server)]
    private void Ability3Rpc()
    {
        GameObject unit = Instantiate(abilities[2], transform.position, Quaternion.identity);
        unit.GetComponent<NetworkObject>().Spawn(true);
        unit.GetComponent<OnlineTarget>().SetNetworkIdRpc(GetId());
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
            Destroy(ability);
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
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Electric Orb";
        SetPowerDescription(0, "[Q] Creates an orb that will damage nearby enemies over time");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Freeze Blast";
        SetPowerDescription(1, "[W] Damages and freezes enemies in a set area.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Steel Call";
        SetPowerDescription(2, "[E] Summon a Steel knight, an long-forgotten machine, for a short while.");
    }
}
