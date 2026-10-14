using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GridManager : MonoBehaviour
{
    void Start()
    {
        GameEvents.current.OnGridUpdate += OnGridUpdate;
    }

    void OnGridUpdate(GraphNode node) => StartCoroutine(MakeNodeWalkable(node));

    IEnumerator MakeNodeWalkable(GraphNode node)
    {
        yield return new WaitForSeconds(1f);
        node.Walkable = true;
    }
}
