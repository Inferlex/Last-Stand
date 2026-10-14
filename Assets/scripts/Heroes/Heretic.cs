using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Heretic : Hero
{
    public GameObject[] abilities;
    [SerializeField] private GameObject InfectExplosion;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Aura of Decay";
        SetPowerDescription(0, "[Q] Weakens nearby enemies with aura");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Infection";
        SetPowerDescription(1, "[W] Infect enemies. Infected explode upon death");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Tentacle Swarm";
        SetPowerDescription(2, "[E] Selected area will be under attack of tentacles for limited time");
    }

    public new void UseAbility(int index)
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

    private new void LevelUp()
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
        SetHp(maxHp);
    }

    private new void Ability1()
    {
        abilities[0].GetComponent<DecayAura>().Use();
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[1]);
    }

    private new void Ability3()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[2]);
    }

    private void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 5))
        {
            ability.GetComponent<Ability>().SetEnabled(false);
            if (ability.GetComponent<Ability>().index == 1)
            {
                ability.GetComponent<Infect>().SetHero(this);
                ability.GetComponent<Infect>().Activate();
                ability.GetComponent<SpriteRenderer>().enabled = true;
                StartCoroutine(Cooldown(1, abilityCooldown[1]));
                SetAbilityRadiusVisibility(false);
            }

            if (ability.GetComponent<Ability>().index == 2)
            {               
                ability.GetComponent<TentacleSwarm>().Activate();
                ability.GetComponent<SpriteRenderer>().enabled = false;
                StartCoroutine(Cooldown(2, abilityCooldown[2]));
                SetAbilityRadiusVisibility(false);
            }
        }
        else
        {
            GameEvents.current.ShowMessage("Ability is not within range");
            Destroy(ability);
            SetAbilityRadiusVisibility(false);
        }
    }

    public GameObject GetExplosion() => InfectExplosion;
}
