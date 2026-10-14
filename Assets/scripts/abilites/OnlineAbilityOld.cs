using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineAbilityOld : NetworkBehaviour
{

    public int index;
    public bool isTarget; //will ability create a target 
    private bool canBeBuilt = true;
    private OnlineHero hero;

    private void Start()
    {
        if(!IsOwner)
            GetComponent<SpriteRenderer>().color = Color.clear;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
        {
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
    }

    void Update()
    {
        //if (!IsOwner) return; //jinak se to pohybuje u ostatních
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
        {
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
        if (IsOwner)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (canBeBuilt)
                {
                    //GameEvents.current.TargetAbility(gameObject);
                    ActivateRpc();
                }
                else
                {
                    //GameEvents.current.ShowMessage("Cannot use this ability here!");
                    GameEvents.current.ShowMessage("Cannot use this ability here!/" + hero.GetId()); //please do the id shit here
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                GameEvents.current.CancelAbility();
                Destroy(gameObject);
            }
        }
    }

    void ActivateRpc()
    {
        //GameEvents.current.TargetAbility(hero.GetId());
        //hero.TargetAbility();
        //Debug.Log("activate");
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (isTarget & !target.CompareTag("Map") && !target.CompareTag("BuildingArea"))
        {
            GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 100);
            canBeBuilt = false;
        }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (isTarget & !target.CompareTag("Map") && !target.CompareTag("BuildingArea"))
        {
            GetComponent<SpriteRenderer>().color = new Color32(0, 255, 0, 100);
            canBeBuilt = true;
        }
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }

    public void SetHero(OnlineHero value)
    {
        hero = value;
    }

    public int GetId()
    {
        return hero.GetId();
    }
}
