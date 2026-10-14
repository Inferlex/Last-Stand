using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Src.Controls;
using Assets.Src.Units;
using Assets.Src;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Tutorial : MonoBehaviour
{

    private Text text;
    private PlayerManager player;
    private int unitsMade;
    private bool generatorBuilt = false;
    private bool refineryBuilt = false;
    private bool abilityUsed = false;
    public Enemy enemy;
    private int enemies = 0;
    public GameObject highlight;
    public WaveManager waveManager;
    private Queue<string> texts = new Queue<string>();
    private int read = 0;
    private int job = 0;
    private bool done = true;
    public GameObject continueButton;

    void Start()
    {
        GameEvents.current.OnDeath += OnDeath;
        GameEvents.current.OnUnitCreation += OnUnitCreation;
        GameEvents.current.OnBuildCreation += OnBuildCreation;
        GameEvents.current.OnUseAbility += OnUseAbility;
        foreach(string text in File.ReadAllLines("TutorialText.txt"))
        {
            texts.Enqueue(text);
        }

        player = FindObjectOfType<PlayerManager>();
        text = player.transform.GetChild(1).GetChild(3).GetChild(14).GetComponent<Text>();
        text.gameObject.SetActive(true);
        continueButton = player.transform.GetChild(1).GetChild(2).GetChild(22).gameObject;
        continueButton.SetActive(true);
        continueButton.GetComponent<Button>().onClick.AddListener(Next);

        StartCoroutine(ShowText(texts.Dequeue(), read));      
    }

    private IEnumerator ShowText(string newText, int index)
    {
        text.text = "";
        
        foreach(char symbol in newText)
        {
            if (symbol.Equals('+'))
            {
                done = false;
                job++;
                StartCoroutine(Action());
                continue;
            }

            if (index == read)
            {
                text.text += symbol;
                yield return new WaitForSeconds(0.02f);
            }
            else
            {
                break;
            }
        }
    }

    private void OnUnitCreation(Unit unit)
    {
        unitsMade++;
    }

    private void OnBuildCreation(Building building)
    {
        if(building.targetName == "Generator")
        {
            generatorBuilt = true;
        }
        if(building.targetName == "Refinery")
        {
            refineryBuilt = true;
        }
    }

    private void OnDeath(Target target)
    {
        if (target.GetId() != 0)
        {
            enemies--;
        }
    }

    private void OnUseAbility(int index)
    {
        if(index == 0)
            abilityUsed = true;
    }

    public void Next()
    {
        if (done && read < File.ReadAllLines("TutorialText.txt").Length - 1)
        {
            read++;
            if (read == File.ReadAllLines("TutorialText.txt").Length - 1)
            {
                waveManager.enabled = true;
                continueButton.SetActive(false);
            }

            StartCoroutine(ShowText(texts.Dequeue(), read));
        }
    }

    private IEnumerator Action()
    {
        continueButton.SetActive(false);
        switch (job)
        {
            case 1:
                while (!player.GetSelectedUnit() || player.GetSelectedUnit().targetName != "Commander")
                {                    
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 2:
                while (!player.GetSelectedUnit() || !(player.GetSelectedUnit().GetComponent<Unit>() && player.GetSelectedUnit().GetComponent<Unit>().GetFlowPath() != null)) // player.GetSelectedUnit().transform.position.x == player.GetSelectedUnit().GetComponent<UnitAI>().target.position.x)
                {     
                    yield return new WaitForSeconds(0.5f);
                }
                Debug.Log(player.GetSelectedUnit().GetComponent<Unit>().GetFlowPath());
                break;
            case 3:
                while (!player.GetSelectedUnit() || !player.GetSelectedUnit().CompareTag("PlayerBuilding"))
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 4:
                while (unitsMade < 4)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 5:
                while (player.GetUnitCount() < 3)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 6:
                while (player.GetGroup(0).Count < 1)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 7:
                generatorBuilt = false;
                while (!generatorBuilt)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 8:
                generatorBuilt = false;
                while (!generatorBuilt)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 9:
                highlight.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 100);
                refineryBuilt = false;
                while (!refineryBuilt)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                highlight.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
                break;
            case 10:
                enemies++;
                Instantiate(enemy, new Vector2(player.hero.transform.position.x - 2, player.hero.transform.position.y), Quaternion.identity);
                while (enemies != 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 11:
                while (!abilityUsed)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                break;
        }
        done = true;
        continueButton.SetActive(true);
        Next();
    }
}
