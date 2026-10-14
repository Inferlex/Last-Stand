using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class EnemyManagaer : MonoBehaviour
{
    private List<Unit> playerUnits = new List<Unit>();
    private List<Building> playerBuildings = new List<Building>();

    void Start()
    {
        GameEvents.current.OnBuildCreation += OnBuildCreation;
        GameEvents.current.OnUnitCreation += OnUnitCreation;
        GameEvents.current.OnDeath += OnDeath;

        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            if (unit.tag == "PlayerUnit" && unit.GetId() != 99)
                playerUnits.Add(unit);
        }

        foreach (Building building in FindObjectsOfType<Building>())
        {
            playerBuildings.Add(building);
        }
    }

    public List<Unit> GetPlayerUnits()
    {
        return playerUnits;
    }

    public List<Building> GetPlayerBuildings()
    {
        return playerBuildings;
    }

    private void OnBuildCreation(Building building)
    {
        playerBuildings.Add(building);
    }

    private void OnUnitCreation(Unit unit)
    {
        playerUnits.Add(unit);
    }

    private void OnDeath(Target target)
    {
        if (target.GetComponent<Unit>())
        {
            if (target.CompareTag("PlayerUnit"))
                playerUnits.Remove(target.GetComponent<Unit>());
        }
        else
        {
            playerBuildings.Remove(target.GetComponent<Building>());
        }
    }

}
