using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Commander : Hero
{
    public GameObject[] abilities;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Battle Cry";
        SetPowerDescription(0, "[Q] Temporalily increases damage of nearby friendly units.");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Stun Grenade";
        SetPowerDescription(1, "[W] Throws a grenade that reduces speed and damage of nearby enemies.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Aura of Victory";
        SetPowerDescription(2, "[E] Envelop yourself in aura that turns nearby friednly units invincible.");
        abilities[2].GetComponent<InvincibleAura>().SetHero(this);
        abilities[0].GetComponent<Aura>().SetHero(this);
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
                StartCoroutine(Cooldown(2, abilityCooldown[2]));
                break;
        }
    }

    private new void LevelUp()
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
        SetHp(maxHp);
    }

    private new void Ability1()
    {      
        abilities[0].GetComponent<Aura>().Use();
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[1], transform.position, Quaternion.identity);
    }

    private new void Ability3()
    { 
        abilities[2].GetComponent<InvincibleAura>().Use();
    }

    private void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 5))
        {
            ability.GetComponent<Ability>().SetEnabled(false);
            if (ability.GetComponent<Ability>().index == 1)
            {               
                ability.GetComponent<Grenade>().Activate();
                StartCoroutine(Cooldown(1, abilityCooldown[1]));
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
}
