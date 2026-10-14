using Assets.Src;
using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineBlueprint : OnlineTarget
{
    RaycastHit hit;
    Vector3 movePoint;
    public GameObject prefab;
    public bool isBuilding = false;
    public float buildingTime;
    public BoxCollider2D bc;
    bool canBeBuilt = true;
    int touching = 0;
    int areaTouching = 0;
    bool isOnArea = false;
    //bool isInControl = false; // to je jen pro online
    private int playerId;
    private int buildId;

    new void Start()
    {
        GameEvents.current.OnBlueprintGeneratorStart += OnBlueprintGeneratorStart;
        bc = gameObject.GetComponent<BoxCollider2D>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
        {
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
        
        if(GetComponent<OnlineRefineryBlueprint>() != null)
        {
            BlueprintStart(1);
            SetCanBeBuilt(false); //used, so you cant build at incorrect place
        }
    }

    public float RoundPosition(float value)
    {
        value /= 2;
        value = Mathf.Round(value);
        value *= 2;
        return value;
    }

    void Update()
    {
        if (!isBuilding) //checks, if it was placed down
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
            {
                transform.position = new Vector2(RoundPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).x), RoundPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).y));
            }
            if(canBeBuilt && isOnArea)
            {
                GetComponent<SpriteRenderer>().color = new Color32(0, 255, 0, 100);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 100);
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (canBeBuilt && isOnArea)
                {
                    isBuilding = true;
                    BlueprintStart(0);
                    GameEvents.current.StartBuilding(true);
                    SpawnBuilding();
                    Destroy(gameObject);
                }
                else
                {
                    GameEvents.current.ShowMessage("You cant build here/" + playerId);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                BlueprintStart(0);
                GameEvents.current.StartBuilding(false);

                Destroy(gameObject);
            }
        }
    }

    public void SpawnBuilding()
    {
        GameEvents.current.PreBuilding(new float[4] { buildId, transform.position.x, transform.position.y, playerId });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {    
        if (collision.CompareTag("BuildingArea"))
        {
            if (collision.GetComponent<Deatacher>().parent.GetComponent<OnlineBase>().GetId() != playerId) return;
            isOnArea = true;
            areaTouching++;
            return;
        }
        if (!collision.CompareTag("GeneratorRadius") && collision.GetComponent<OnlineTurretRadius>() == null && collision.GetComponent<OnlineUnitRadius>() == null)
        {
            touching++;
        }
        else if (targetName == "Generator" || targetName == "Energy Crystal")
        {
            if (collision.GetComponent<Deatacher>() != null && collision.GetComponent<Deatacher>().parent.GetComponent<OnlineGenerator>().GetId() == playerId)
                touching++;
        }
        if (touching > 0 && !isBuilding)
        {
            canBeBuilt = false;
        }
        //Debug.Log("enter " + collision + " " + touching);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {   
        if (collision.CompareTag("BuildingArea"))
        {
            if (collision.GetComponent<Deatacher>().parent.GetComponent<OnlineBase>().GetId() != playerId) return;
            areaTouching--;
            if(areaTouching == 0)
                isOnArea = false;
            return;
        }
        if (!collision.CompareTag("GeneratorRadius") && collision.GetComponent<OnlineTurretRadius>() == null && collision.GetComponent<OnlineUnitRadius>() == null)
        {
            touching--;
        }
        else if (targetName == "Generator" || targetName == "Energy Crystal")
        {
            if (collision.GetComponent<Deatacher>() != null && collision.GetComponent<Deatacher>().parent.GetComponent<OnlineGenerator>().GetId() == playerId)
                touching--;
        }
        if (touching == 0)
        {
            canBeBuilt = true;
        }
        //Debug.Log("exit " + collision + " " + touching);
    }

    public void BlueprintStart(int value)
    {
        if (targetName == "Generator" || targetName == "Energy Crystal")       
            GameEvents.current.OnlineBlueprintGeneratorStart(new int[2] {value, playerId});

        GameEvents.current.OnlineBlueprintStart(new int[2] {value, playerId}); 
    }

    public void OnBlueprintGeneratorStart(bool value)
    {
        if (this != null)
        {
            if ((targetName == "Generator" || targetName == "Energy Crystal") && transform.GetChild(0).gameObject)
                if (value)
                {
                    transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 255);
                }
                else
                {
                    transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 0);
                }
        }
    }

    public void SetPlayerId(int i)
    {
        playerId = i;
        BlueprintStart(1);
    }

    public void SetBuildId(int i)
    {
        buildId = i;
    }

    public bool GetCanBeBuilt()
    {
        return canBeBuilt;
    }

    public void SetCanBeBuilt(bool value)
    {
        canBeBuilt = value;
    }

    public bool GetIsOnArea()
    {
        return isOnArea;
    }

    public void SetIsOnArea(bool value)
    {
        isOnArea = value;
    }

    public int GetPlayerId() => playerId;
}
