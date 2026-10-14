using Assets.Src;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineRefineryBlueprint : OnlineBlueprint
{

    /*new void Start()
    { 
        BlueprintStart(1);
        SetCanBeBuilt(false); //used, so you cant build at incorrect place
    }*/

    // Update is called once per frame
    void Update()
    {
        if (!isBuilding) //was the blueprint placed down?
        {
            if (GetCanBeBuilt() && GetIsOnArea())
            {
                GetComponent<SpriteRenderer>().color = new Color32(0, 255, 0, 100);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 100);
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
            {
                transform.position = new Vector2(RoundPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).x), RoundPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).y));
            }

                if (Input.GetMouseButtonDown(0))
            {
                if (GetCanBeBuilt() && GetIsOnArea())
                {
                    isBuilding = true;
                    BlueprintStart(0);
                    GameEvents.current.StartBuilding(true);
                    SpawnBuilding();
                    Destroy(gameObject);
                }
                else
                {
                    GameEvents.current.ShowMessage("You cant build here/" + GetPlayerId());
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BuildingArea"))
        {
            SetIsOnArea(true);
            return;
        }
        if (collision.CompareTag("Resource"))
            SetCanBeBuilt(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("BuildingArea"))
        {
            SetIsOnArea(false);
            return;
        }
        if (collision.CompareTag("Resource"))
            SetCanBeBuilt(false);
    }
}
