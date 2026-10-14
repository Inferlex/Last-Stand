using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineArtificier : OnlineHero
{
    public GameObject fireEffect;
    public GameObject[] abilities;
    private int AvailableMines = 3;
    public Transform attackPosition;
    public GameObject[] preAbilities;

    private new void Start()
    {
        base.Start();
        GameEvents.current.OnUseAbility += UseAbility;
        GameEvents.current.OnLevelUp += LevelUp;
        GameEvents.current.OnTargetAbility += TargetAbility;
        GameEvents.current.OnMineDestroy += MineDestroy;
        if (IsOwner)
        {
            powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Mini Turret";
            SetPowerDescription(0, "[Q] Creates a small turret.");
            powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Explosive Mines";
            SetPowerDescription(1, "[W] Creates Mines that will explode when enemy is near.");
            powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Nuclear Blast";
            SetPowerDescription(2, "[E] Self-explanatory. Does severe damage to EVERYONE.");
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
        SetHealthRpc(maxHp);
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
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[2], transform.position, Quaternion.identity).GetComponent<PreAbility>().SetHero(this);
    }

    public override void TargetAbility(GameObject ability)
    {
        if (AbilityWithinRange(ability.transform.position, 6))
        {
            int abilityId = ability.GetComponent<PreAbility>().index;

            if (abilityId == 1)
            {
                if (AvailableMines > 0)
                {
                    AvailableMines--;
                }
                else
                {
                    GameEvents.current.ShowMessage("You can't deploy any more mines./" + GetId());
                    return;
                }
            }

            TargetAbilityRpc(ability.transform.position.x, ability.transform.position.y, abilityId);
            StartCoroutine(Cooldown(abilityId, abilityCooldown[abilityId]));
            SetAbilityRadiusVisibility(false);
        }
        else
        {
            GameEvents.current.ShowMessage("Ability is not within range/" + GetId());
            //Destroy(ability);
            SetAbilityRadiusVisibility(false);
        }
    }

    [Rpc(SendTo.Server)]
    private void TargetAbilityRpc(float x, float y, int id)
    {
        GameObject ability = Instantiate(abilities[id], new Vector3(x, y, -1f), transform.rotation);
        ability.GetComponent<NetworkObject>().Spawn(true);
        switch (id)
        {
            case 0:
                ability.GetComponent<OnlineTarget>().SetNetworkIdRpc(GetId());
                break;
            case 1:
                ability.GetComponent<OnlineMines>().SetId(GetId());
                break;
            case 2:
                ability.GetComponent<OnlineAbility>().SetId(GetId());
                ability.GetComponent<OnlineAbility>().Activate();
                break;
        }
    }

    public void ShowAttack(OnlineTarget target)
    {
        GameObject effect = Instantiate(fireEffect);
        effect.GetComponent<LineController>().SetPoints(attackPosition.transform, target.transform);
    }

    private void MineDestroy()
    {
        AvailableMines++;
    }

    public override void SetButtons()
    {
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Mini Turret";
        SetPowerDescription(0, "[Q] Creates a small turret.");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Explosive Mines";
        SetPowerDescription(1, "[W] Creates Mines that will explode when enemy is near.");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Nuclear Blast";
        SetPowerDescription(2, "[E] Self-explanatory. Does severe damage to EVERYONE.");
    }
}
