using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stalker : Hero
{
    public GameObject[] abilities;
    [SerializeField] private GameObject cloneExplosion;
    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Corrosive Puddle";
        SetPowerDescription(0, "[Q] Temporalily increases damage of nearby friendly units.");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Chameleon skin";
        SetPowerDescription(1, "[W] temporary invisibility.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Chaos";
        SetPowerDescription(2, "[E] Creates three copies of itself. Copies are weaker and explode on death");
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
                StartCoroutine(Cooldown(1, abilityCooldown[1]));
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
                damage += 14;
                fireRate -= 0.1f;
                maxHp += 120;
                break;
        }
        SetHp(maxHp);
    }

    private new void Ability1()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(abilities[0], transform.position, Quaternion.identity);
    }

    private new void Ability2()
    {
        StartCoroutine(Invisible());
    }

    private new void Ability3()
    {
        for (int i = 0;i<3;i++)
        {
            Instantiate(abilities[2], transform.position, Quaternion.identity).GetComponent<Unit>().Infect(GetId(), cloneExplosion);
        }
    }

    private IEnumerator Invisible()
    {
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 100);
        GetComponent<CapsuleCollider2D>().enabled = false;
        yield return new WaitForSeconds(15f);
        GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        GetComponent<CapsuleCollider2D>().enabled = true;
    }

    private void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 5))
        {
            ability.GetComponent<Ability>().SetEnabled(false);
            if (ability.GetComponent<Ability>().index == 0)
            {               
                ability.GetComponent<CorrosivePuddle>().Activate(this);
                StartCoroutine(Cooldown(0, abilityCooldown[0]));
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
