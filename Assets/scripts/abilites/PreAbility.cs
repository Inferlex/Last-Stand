using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PreAbility : MonoBehaviour
{
    public int index;
    public bool isTarget; //will ability create a target 
    private bool canBeBuilt = true;
    private OnlineHero hero;
    //[SerializeField] private GameObject ability;

    private void Start()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
        {
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero))
        {
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (canBeBuilt)
            {
                hero.TargetAbility(gameObject);
                Destroy(gameObject);
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

    public void SetHero(OnlineHero value)
    {
        hero = value;
    }
}
