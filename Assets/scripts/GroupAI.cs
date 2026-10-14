using Assets.Src.Units;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroupAI : MonoBehaviour
{
    public Transform target;

    public float nextWaipointDistance = 3f;

    Path path;

    private List<Unit> units;

    private bool isCalculating;


    public void StartNewPath(Vector2 pos, List<Unit> units)
    {
        this.units = units;
        target.position = pos;
        isCalculating = true;
        GetComponent<Seeker>().StartPath(transform.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p) //called when path is calculated
    {
        if (!p.error) //if it doesnt end with error
        {
            path = p;
        }
        isCalculating = false;
        GoAroundObstacle();
        foreach(var unit in units)
        {
            unit.GetComponent<UnitAI>().SetPathToFollow(p, unit.transform.position - transform.position);
        }
        Destroy(gameObject);
    }

    private void GoAroundObstacle() //modifies the path so it doesnt go closely around obstacles
    {
        List<Vector2> VectorDirections = new List<Vector2>();
        List<Vector3> newPathVectors = new List<Vector3>();
        for (int i = 1; i < path.vectorPath.Count; i++)
        {
            VectorDirections.Add(path.vectorPath[i] - path.vectorPath[i - 1]);
            if (i >= 2)
            {
                Vector2 NewVector = VectorDirections[i - 1] + VectorDirections[i - 2]; //vectorDirections have count of i-1
                var avgHit = (Vector2)path.vectorPath[i - 2] + NewVector / 2;
                var hit = avgHit + Vector2.Perpendicular(NewVector);
                var hit2 = avgHit + (Vector2.Perpendicular(NewVector) * -1);
                bool walkable = AstarPath.active.GetNearest(hit).node.Walkable;
                bool walkable2 = AstarPath.active.GetNearest(hit2).node.Walkable;
                if (!walkable && walkable2)
                {
                    newPathVectors.Add(hit2);
                }
                else if (walkable && !walkable2)
                {
                    newPathVectors.Add(hit);
                }
                else
                {
                    newPathVectors.Add(path.vectorPath[i-1]);
                }
            }
        }
        newPathVectors.Add(path.vectorPath.Skip(path.vectorPath.Count - 2).ToList()[0]);
        newPathVectors.Add(path.vectorPath.Last());

        for(int i = 1; i < newPathVectors.Count - 1; i++)
        {
            newPathVectors[i] = newPathVectors[i-1] + ((newPathVectors[i] - newPathVectors[i - 1]) + (newPathVectors[i + 1] - newPathVectors[i]))/2;
        }

        for(int i = 0; i < path.vectorPath.Count; i++)
        {
            path.vectorPath[i] = newPathVectors[i];
        }
    }

    public bool GetIsCalculating() => isCalculating;
}
