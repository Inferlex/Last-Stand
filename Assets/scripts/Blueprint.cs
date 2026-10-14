using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Blueprint : Target
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

    new void Start()
    {
        GameEvents.current.OnBlueprintGeneratorStart += OnBlueprintGeneratorStart;
        base.Start();
        BlueprintStart(true);
        bc = gameObject.GetComponent<BoxCollider2D>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
        {
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
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
        if (!isBuilding && Time.timeScale != 0) //checks, if it was placed down or if the game was paused
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
            {
                transform.position = new Vector2(RoundPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).x), RoundPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).y));
            }
            if (canBeBuilt && isOnArea)
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
                    GameEvents.current.StartBuilding(true);
                    StartCoroutine(Construction());
                }
                else
                {
                    GameEvents.current.ShowMessage("You cant build here");
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                BlueprintStart(false);
                GameEvents.current.StartBuilding(false);
                Destroy(gameObject);
            }
        }
    }

    public IEnumerator Construction()
    {
        BlueprintStart(false);
        GetComponent<SpriteRenderer>().color = new Color32(0, 255, 255, 100);
        if (targetName == "Generator" || targetName == "Energy Crystal")
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(buildingTime);

        if (prefab.GetComponent<OnlineBuilding>())
        {
            SpawnObject();
        }
        else
        {
            Instantiate(prefab, transform.position + new Vector3(0f, 0f, -1f), transform.rotation);
        }
        Destroy(gameObject);
    }

    private void SpawnObject()
    {
        GameObject building = Instantiate(prefab, transform.position + new Vector3(0f, 0f, -1f), transform.rotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.GetComponent<TurretRadius>() && !collision.GetComponent<UnitRadius>())
        {
            if (collision.CompareTag("BuildingArea"))
            {
                isOnArea = true;
                areaTouching++;
                return;
            }
            if (!collision.CompareTag("GeneratorRadius"))
            {
                touching++;
            }
            else if (targetName == "Generator" || targetName == "Energy Crystal")
            {
                touching++;
            }
            if (touching > 0 && !isBuilding)
            {
                canBeBuilt = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.GetComponent<TurretRadius>() && !collision.GetComponent<UnitRadius>())
        {
            if (collision.CompareTag("BuildingArea"))
            {
                areaTouching--;
                if (areaTouching == 0)
                    isOnArea = false;
                return;
            }
            if (!collision.CompareTag("GeneratorRadius"))
            {
                touching--;
            }
            else if (targetName == "Generator" || targetName == "Energy Crystal")
            {
                touching--;
            }
            if (touching == 0)
            {
                canBeBuilt = true;
            }
        }
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

    public void BlueprintStart(bool value)
    {
        if (targetName == "Generator" || targetName == "Energy Crystal")
            GameEvents.current.BlueprintGeneratorStart(value);

        GameEvents.current.BlueprintStart(value);

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
}
