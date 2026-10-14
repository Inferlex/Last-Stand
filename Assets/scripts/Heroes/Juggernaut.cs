using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Juggernaut : Hero
{
    public GameObject[] abilities;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Slam";
        SetPowerDescription(0, "[Q] Damages enemies around you");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Rising Spikes";
        SetPowerDescription(1, "[W] creates spikes in line");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Berserk";
        SetPowerDescription(2, "[E] Channel your rage. Temporary invincibility and increased attack.");
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
                damage += 16;
                fireRate -= 0.1f;
                maxHp += 120;
                break;
        }
        SetHp(maxHp);
    }

    private new void Ability1()
    {
        //abilities[0].GetComponent<Aura>().Use();
        GameObject ability = Instantiate(abilities[0], transform.position, Quaternion.identity);
        ability.GetComponent<SplashDamage>().Activate(100, GetId());
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[1], transform.position, Quaternion.identity);
    }

    private new void Ability3()
    {
        StartCoroutine(Berserk());
    }

    private IEnumerator Berserk()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        damage += 60;
        fireRate -= 0.5f;
        SetInvincible(true);
        yield return new WaitForSeconds(15f);
        damage -= 60;
        fireRate += 0.5f;
        SetInvincible(false);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 5))
        {
            ability.GetComponent<Ability>().SetEnabled(false);
            if (ability.GetComponent<Ability>().index == 1)
            {
                ability.GetComponent<Spikes>().SetCreator(gameObject);
                //ability.GetComponent<Spikes>().Activate();
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
