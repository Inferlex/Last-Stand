using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Artificier : Hero
{
    public GameObject fireEffect;
    public GameObject[] abilities;
    private int AvailableMines = 3;
    public Transform attackPosition;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        GameEvents.current.OnMineDestroy += MineDestroy;
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Mini Turret";
        SetPowerDescription(0, "[Q] Creates a small turret.");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Explosive Mines";
        SetPowerDescription(1, "[W] Creates Mines that will explode when enemy is near.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Nuclear Blast";
        SetPowerDescription(2, "[E] Self-explanatory. Does severe damage to EVERYONE.");
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
                damage += 10;
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
                abilityCooldown[1] -= 4f;
                break;
            case 5:
                damage += 15;
                fireRate -= 0.1f;
                maxHp += 100;
                abilityCooldown[0] -= 6f;
                abilityCooldown[1] -= 4f;
                break;
            case 6:
                damage += 20;
                fireRate -= 0.1f;
                maxHp += 100;
                break;
        }
        SetHp(maxHp);
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
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[2]);
    }

    private void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 6))
        {
            ability.GetComponent<Ability>().SetEnabled(false);
            if (ability.GetComponent<Ability>().index == 0)
            {
                ability.GetComponent<ArtificierObject>().Activate();
                StartCoroutine(Cooldown(0, abilityCooldown[0]));
                SetAbilityRadiusVisibility(false);
            }
            else if (ability.GetComponent<Ability>().index == 1)
            {
                if (AvailableMines > 0)
                {
                    GameObject mine = ability.GetComponent<ArtificierObject>().Activate();
                    ability.GetComponent<ArtificierObject>().Die();
                    mine.GetComponent<Mines>().SetHero(this);
                    AvailableMines--;
                    StartCoroutine(Cooldown(1, abilityCooldown[1]));
                    SetAbilityRadiusVisibility(false);
                }
                else
                {
                    GameEvents.current.ShowMessage("You can't deploy any more mines.");
                    Destroy(ability);
                    Ability2();
                }
            }
            else if (ability.GetComponent<Ability>().index == 2)
            {
                ability.GetComponent<Bomb>().Activate();
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

    public void ShowAttack(Target target)
    {
        GameObject effect = Instantiate(fireEffect);
        effect.GetComponent<LineController>().SetPoints(attackPosition.transform, target.transform);
    }

    private void MineDestroy()
    {
        AvailableMines++;
    }
}
