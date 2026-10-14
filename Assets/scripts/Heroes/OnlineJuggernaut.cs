using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnlineJuggernaut : OnlineHero
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
            powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Slam";
            SetPowerDescription(0, "[Q] Damages enemies around you");
            powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Rising Spikes";
            SetPowerDescription(1, "[W] creates spikes in line");
            powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Berserk";
            SetPowerDescription(2, "[E] Channel your rage. Temporary invincibility and increased attack.");
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
        SetHealthRpc(maxHp);
    }

    private new void Ability1()
    {
        Ability1Rpc();
    }

    [Rpc(SendTo.Server)]
    private void Ability1Rpc()
    {
        GameObject ability = Instantiate(abilities[0], transform.position, Quaternion.identity);
        ability.GetComponent<OnlineSplashDamage>().SetUnit(GetComponent<OnlineUnit>());
        ability.GetComponent<NetworkObject>().Spawn();
        ability.GetComponent<OnlineSplashDamage>().Activate(100, GetId());
    }

    private new void Ability2()
    {
        SetAbilityRadiusVisibility(true);
        Instantiate(preAbilities[0], transform.position, Quaternion.identity).GetComponent<PreAbility>().SetHero(this);
    }

    private new void Ability3()
    {
        Ability3Rpc();
    }

    [Rpc(SendTo.Everyone)]
    private void Ability3Rpc()
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

    /*public override void TargetAbility(GameObject ability)
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
    }*/

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
        //ability.GetComponent<OnlineAbility>().SetId(GetId());
        //ability.GetComponent<OnlineAbility>().Activate(this);
        ability.GetComponent<OnlineSpikes>().SetCreator(gameObject);
    }

    public override void SetButtons()
    {
        powerButtons[0].transform.GetChild(0).GetComponent<Text>().text = "Slam";
        SetPowerDescription(0, "[Q] Damages enemies around you");
        powerButtons[1].transform.GetChild(0).GetComponent<Text>().text = "Rising Spikes";
        SetPowerDescription(1, "[W] creates spikes in line");
        powerButtons[2].transform.GetChild(0).GetComponent<Text>().text = "Berserk";
        SetPowerDescription(2, "[E] Channel your rage. Temporary invincibility and increased attack.");
    }
}
