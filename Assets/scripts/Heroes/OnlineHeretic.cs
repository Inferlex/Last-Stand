using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineHeretic : OnlineHero
{
    public GameObject[] abilities;
    [SerializeField] private GameObject[] preAbilities;
    [SerializeField] private GameObject InfectExplosion;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        //GameEvents.current.OnTargetAbility += TargetAbility;
        if (IsOwner)
        {
            powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Aura of Decay";
            SetPowerDescription(0, "[Q] Weakens nearby enemies with aura");
            powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Infection";
            SetPowerDescription(1, "[W] Infect enemies. Infected explode upon death");
            powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Tentacle Swarm";
            SetPowerDescription(2, "[E] Selected area will be under attack of tentacles for limited time");
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
                    StartCoroutine(Cooldown(0, abilityCooldown[0]));
                    break;
                case 1:
                    Ability2();
                    break;
                case 2:
                    Ability3();
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
                fireRate -= 0.1f;
                maxHp += 50;
                break;
            case 3:
                fireRate -= 0.1f;
                maxHp += 50;
                abilityCooldown[0] -= 6f;
                break;
            case 4:
                damage += 8;
                maxHp += 70;
                abilityCooldown[1] -= 6;
                break;
            case 5:
                damage += 10;
                fireRate -= 0.1f;
                maxHp += 90;
                abilityCooldown[0] -= 4f;
                break;
            case 6:
                damage += 14;
                fireRate -= 0.2f;
                maxHp += 120;
                break;
        }
        SetHealthRpc(maxHp);
    }

    private new void Ability1()
    {
        //abilities[0].GetComponent<DecayAura>().Use();
        Ability1Rpc();
    }

    [Rpc(SendTo.Everyone)]
    private void Ability1Rpc()
    {
        abilities[0].GetComponent<OnlineDecayAura>().Use();
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[0]).GetComponent<PreAbility>().SetHero(this);
    }

    private new void Ability3()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[1]).GetComponent<PreAbility>().SetHero(this);
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

    public GameObject GetExplosion() => InfectExplosion;

    public override void SetButtons()
    {
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Aura of Decay";
        SetPowerDescription(0, "[Q] Weakens nearby enemies with aura");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Infection";
        SetPowerDescription(1, "[W] Infect enemies. Infected explode upon death");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Tentacle Swarm";
        SetPowerDescription(2, "[E] Selected area will be under attack of tentacles for limited time");
    }
}
