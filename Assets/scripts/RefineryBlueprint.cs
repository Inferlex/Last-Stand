using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefineryBlueprint : Blueprint
{

    new void Start()
    {
        base.Start();
        BlueprintStart(true);
        SetCanBeBuilt(false); //used, so you cant build at incorrect place
    }

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
                    GameEvents.current.StartBuilding(true);
                    StartCoroutine(Construction());
                }
                else
                {
                    GameEvents.current.ShowMessage("You cant build here");
                }
                //Instantiate(prefab, transform.position + new Vector3(0f, 0f, -1f), transform.rotation);
                //Destroy(gameObject);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                BlueprintStart(false);
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
