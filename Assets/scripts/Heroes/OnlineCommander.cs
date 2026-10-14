using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineCommander : OnlineHero
{
    public GameObject[] abilities;
    [SerializeField] private GameObject[] preAbilities;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        //GameEvents.current.OnOnlineTargetAbility += TargetAbility;
        if (IsOwner)
        {
            powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Battle Cry";
            SetPowerDescription(0, "[Q] Temporalily increases damage of nearby friendly units.");
            powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Stun Grenade";
            SetPowerDescription(1, "[W] Throws a grenade that reduces speed and damage of nearby enemies.");
            powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Aura of Victory";
            SetPowerDescription(2, "[E] Envelop yourself in aura that turns nearby friednly units invincible.");
        }
        abilities[2].GetComponent<OnlineInvincibleAura>().SetHero(this);
        abilities[0].GetComponent<OnlineAura>().SetHero(this);
    }

    public override void UseAbility(int index)
    {
        if (!IsOnCooldown(index))
        {
            switch (index)
            {
                case 0:
                    Ability1();
                    StartCoroutine(Cooldown(0, abilityCooldown[0]));
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
                maxHp += 50;
                break;
            case 3:
                fireRate -= 0.1f;
                maxHp += 50;
                abilityCooldown[0] -= 6f;
                break;
            case 4:
                damage += 10;
                maxHp += 100;
                abilityCooldown[1] -= 6;
                break;
            case 5:
                damage += 10;
                fireRate -= 0.1f;
                maxHp += 100;
                abilityCooldown[0] -= 4f;
                break;
            case 6:
                damage += 12;
                fireRate -= 0.2f;
                maxHp += 100;
                break;
        }
        SetHealthRpc(maxHp);
    }

    private new void Ability1()
    {
        //abilities[0].GetComponent<OnlineAura>().Use();
        Ability1Rpc();
    }

    [Rpc(SendTo.Everyone)]
    private void Ability1Rpc()
    {
        abilities[0].GetComponent<OnlineAura>().Use();
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[0], transform.position, Quaternion.identity).GetComponent<PreAbility>().SetHero(this);
        //Ability2Rpc();
    }

    [Rpc(SendTo.Server)]
    private void Ability2Rpc()
    {
        GameObject gobj = Instantiate(abilities[1], transform.position, Quaternion.identity);
        gobj.GetComponent<NetworkObject>().Spawn(true);
        gobj.GetComponent<PreAbility>().SetHero(this);
    }

    private new void Ability3()
    {
        //abilities[2].GetComponent<InvincibleAura>().Use();
        Ability3Rpc();
    }

    [Rpc(SendTo.Everyone)]
    private void Ability3Rpc()
    {
        abilities[2].GetComponent<OnlineInvincibleAura>().Use();
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
        ability.GetComponent<OnlineGrenade>().Activate();
    }

    public override void SetButtons()
    {
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Battle Cry";
        SetPowerDescription(0, "[Q] Temporalily increases damage of nearby friendly units.");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Stun Grenade";
        SetPowerDescription(1, "[W] Throws a grenade that reduces speed and damage of nearby enemies.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Aura of Victory";
        SetPowerDescription(2, "[E] Envelop yourself in aura that turns nearby friednly units invincible.");
    }
}
