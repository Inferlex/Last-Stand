using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Acolyte: Hero
{
    public GameObject fireEffect;
    public GameObject[] abilities;
    public Transform attackPosition;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        //fireEffect.transform.SetParent(null); Maybe add it later, it does shit now
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Electric Orb";
        SetPowerDescription(0, "[Q] Creates an orb that will damage nearby enemies over time");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Freeze Blast";
        SetPowerDescription(1, "[W] Damages and freezes enemies in a set area.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Steel Call";
        SetPowerDescription(2, "[E] Summon a Steel knight, an long-forgotten machine, for a short while.");
    }

    public new void UseAbility(int index)
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

    private new void LevelUp()
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
        SetHp(maxHp);
    }

    public void ShowAttack(Target target)
    {
        GameObject effect = Instantiate(fireEffect);
        effect.GetComponent<LineController>().SetPoints(attackPosition.transform, target.transform);
    }

    private new void Ability1()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[0]);
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[1]);
    }
    
    private new void Ability3()
    {
        Instantiate(abilities[2], transform.position, Quaternion.identity);
    }

    private void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 5))
        {
            ability.GetComponent<Ability>().SetEnabled(false);
            if (ability.GetComponent<Ability>().index == 0)
            {    
                ability.GetComponent<ElectricArc>().Activate(this);
                StartCoroutine(Cooldown(0, abilityCooldown[0]));
                SetAbilityRadiusVisibility(false);
            }
            else if (ability.GetComponent<Ability>().index == 1)
            {
                ability.GetComponent<FreezeBlast>().Activate(this);
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
