using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using System.Linq;
using UnityEngine.UI;

public class Hero : Unit
{
    //public GameObject buffEffect;
    private int level = 1;
    private int[] nextLevel = { 80, 250, 500, 900, 1500 };
    private int currentXP = 0;
    private bool[] abilityUnlocked; // controls if an ability was unlocked
    public List<float> abilityCooldown; //cooldown of an ability
    private bool[] onCooldown; //controls if an ability is on cooldown;
    public List<Button> powerButtons;
    private List<string> powerDescription = new List<string>();
    private bool heroIsDead;
    public GameObject abilityRadius;   

    public void Awake()
    {
        powerDescription = Enumerable.Repeat<string>("", 3).ToList();
        abilityUnlocked = Enumerable.Repeat<bool>(false, 3).ToArray();
        onCooldown = Enumerable.Repeat<bool>(false, 3).ToArray();
        abilityUnlocked[0] = true;
        abilityUnlocked[1] = true; //for testing only
        abilityUnlocked[2] = true; //for testing only    
        foreach(Button button in FindObjectsOfType<Button>())
        {
            if (button.name.Equals("Power1Button"))
                powerButtons[0] = button;
            if (button.name.Equals("Power2Button"))
                powerButtons[1] = button;
            if (button.name.Equals("Power3Button"))
                powerButtons[2] = button;
        }        
        StartCoroutine(Heal());
    }

    new public void Start()
    {
        base.Start();
        GameEvents.current.OnCancelAbility += OnCancelAbility;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    public void UseAbility(int index)
    {
        if (abilityUnlocked[index] == true && !onCooldown[index])
        {
            GameEvents.current.UseAbility(index);
        }
        else
        {
            GameEvents.current.ShowMessage("cant use this ability");
        }
    }

    public void GetXP(int xp)
    {
        currentXP += xp;
        if(level != 6 && currentXP >= nextLevel[level - 1])
        {
            LevelUp();
            GetXP(0);
        }
    }


    public void LevelUp()
    {       
        level++;
        GameEvents.current.ShowMessage(targetName + " has reached level " + level + "!");
        switch (level)
        {
            case 2:
                abilityUnlocked[1] = true;
                powerButtons[1].transform.GetChild(1).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                break;
            case 4:
                abilityUnlocked[2] = true;
                powerButtons[2].transform.GetChild(1).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                break;
        }
        GameEvents.current.LevelUp();
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

    public int GetLevel()
    {
        return level;
    }

    private IEnumerator Heal()
    {
        yield return new WaitForSeconds(5f);
        if(GetHp() >= maxHp - 5)
        {
            SetHp(maxHp);
        }
        else
        {
            SetHp(GetHp() + 10);           
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
}
