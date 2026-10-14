using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using System.Linq;
using UnityEngine.UI;
using Unity.Netcode;

public class OnlineHero : OnlineUnit
{
    private int level = 1;
    private int[] nextLevel = { 80, 250, 500, 800, 1200 };
    private NetworkVariable<int> networkXp = new NetworkVariable<int>(0);
    private bool[] abilityUnlocked; // controls if an ability was unlocked
    public List<float> abilityCooldown; //cooldown of an ability
    private bool[] onCooldown; //controls if an ability is on cooldown;
    public List<Button> powerButtons;
    private List<string> powerDescription = new List<string>(3);
    private bool heroIsDead;
    public GameObject abilityRadius;
    private bool regeneratesHealth = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkXp.OnValueChanged += OnXpChange;
    }

    public void OnXpChange(int prev, int current)
    {
        networkXp.Value = current;
        if (level != 6 && networkXp.Value >= nextLevel[level - 1])
        {
            LevelUp();
        }
    }

    new public void Awake()
    {
        base.Awake();
        powerDescription = Enumerable.Repeat<string>("", 3).ToList();
        abilityUnlocked = Enumerable.Repeat<bool>(false, 3).ToArray();
        onCooldown = Enumerable.Repeat<bool>(false, 3).ToArray();
        abilityUnlocked[0] = true;
        //abilityUnlocked[1] = true; //for testing only
        //abilityUnlocked[2] = true; //for testing only    
    }

    public void SetPowers(Button[] buttons)
    {
        powerButtons[0] = buttons[0];
        powerButtons[1] = buttons[1];
        powerButtons[2] = buttons[2];
    }

    new public void Start()
    {
        base.Start();
        GameEvents.current.OnCancelAbility += OnCancelAbility;
        RegenerateRpc();
    }

    new void Update()
    {
        base.Update();
    }

    /*public void UseAbility(int index)
    {
        if (abilityUnlocked[index] == true && !onCooldown[index])
        {
            GameEvents.current.UseAbility(index);
        }
        else
        {
            GameEvents.current.ShowMessage("cant use this ability/" + GetId());
        }
    }*/

    virtual public void UseAbility(int index)
    {
        Debug.Log("this is virtual");
    }

    [Rpc(SendTo.Server)]
    public void GetXPRpc(int xp)
    {
        networkXp.Value += xp;
    }

    public void LevelUp()
    {       
        level++;
        GameEvents.current.ShowMessage(targetName + " has reached level " + level + "!/" + GetId());
        switch (level)
        {
            case 2:
                abilityUnlocked[1] = true;
                if(powerButtons[1] != null)
                    powerButtons[1].transform.GetChild(1).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                break;
            case 4:
                abilityUnlocked[2] = true;
                if (powerButtons[2] != null)
                    powerButtons[2].transform.GetChild(1).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                break;
        }
        //GameEvents.current.LevelUp();
        LevelUpStats();
    }

    public virtual void LevelUpStats()
    {
        Debug.Log("override this");
    }

    public void Ability1()
    {
        Debug.Log("this ability is not implemented");
    }

    public void Ability2()
    {
        Debug.Log("this ability is not implemented");
    }

    public void Ability3()
    {
        Debug.Log("this ability is not implemented");
    }   


    public IEnumerator Cooldown(int index, float seconds)
    {
        onCooldown[index] = true;
        powerButtons[index].transform.GetChild(1).GetComponent<Image>().color = new Color32(0, 0, 0, 150);
        for(int i = 0; i < seconds; i++)
        {
            powerButtons[index].transform.GetChild(2).GetComponent<Text>().text = (seconds - i).ToString();
            yield return new WaitForSeconds(1f);
        }
        powerButtons[index].transform.GetChild(2).GetComponent<Text>().text = "";
        onCooldown[index] = false;
        powerButtons[index].transform.GetChild(1).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
    }

    public bool IsOnCooldown(int index) => onCooldown[index];

    public int GetLevel()
    {
        return level;
    }

    [Rpc(SendTo.Server)]
    private void RegenerateRpc()
    {
        if (!regeneratesHealth)
        {
            regeneratesHealth = true;
            StartCoroutine(Heal());
        }
    }

    private IEnumerator Heal()
    {
        yield return new WaitForSeconds(5f);
        if(GetHp() >= maxHp - 5)
        {
            SetHealthRpc(maxHp);
        }
        else
        {
            SetHealthRpc(GetHp() + 10);           
        }
        healthBar.ChangeSprites(maxHp, GetHp());
        StartCoroutine(Heal());
    }

    public string GetPowerDescription(int index)
    {
        return powerDescription[index];
    }

    public void SetPowerDescription(int index, string text)
    {
        powerDescription[index] = text;
    }

    new public bool GetIsDead()
    {
        return heroIsDead;
    }

    public void SetIsDead(bool value)
    {
        heroIsDead = value;
    }

    public void ResetCooldowns()
    {
        onCooldown = Enumerable.Repeat<bool>(false, 3).ToArray();
    }

    public void SetAbilityRadiusVisibility(bool value)
    {
        if (value)
            abilityRadius.GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 60);
        else
            abilityRadius.GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 0);
    }

    public bool AbilityWithinRange(Vector2 position, int distance)
    {
        if(Vector2.Distance(position, transform.position) < distance)
        {
            return true;
        }
        return false;
    }

    private void OnCancelAbility()
    {
        SetAbilityRadiusVisibility(false);
    }

    public virtual void SetButtons()
    {
        Debug.Log("override this method");
    }

    virtual public void TargetAbility(GameObject ability)
    {
        Debug.Log("override this");
    }

    public void TeleportToBuilding(Vector2 vc)
    {
        transform.position = vc;
    }
}
