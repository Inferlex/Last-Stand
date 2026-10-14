using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private byte dir;
    private ushort cost;
    private int ownedByTile;
    private Node[] neighbors;
    private Vector2 pos;
    private float area;

    public Node()
    {
        dir = 0;
        neighbors = new Node[8];
        area = 0.7f;
        cost = ushort.MaxValue;
    }

    public Node(int ownedByTile, Vector2 pos, byte dir, ushort cost) //if anything goes wrong, add back neighbors
    {
        this.ownedByTile = ownedByTile;
        this.pos = pos;
        area = 0.7f;
        this.cost = cost;
        this.dir = dir;
    }

    public void SetPos(Vector2 pos)
    {
        this.pos = pos;
    }

    public Vector2 GetPos()
    {
        return pos;
    }

    public float GetArea()
    {
        return area;
    }

    public void SetCost(ushort c)
    {
        cost = c;
    }

    public ushort GetCost()
    {
        return cost;
    }

    public void SetNeighbor(Node node, int i)
    {
        neighbors[i] = node;
    }

    public Node GetNeighbor(int i)
    {
        return neighbors[i];
    }

    public List<int> GetNeighborPos(int i)
    {
        //return neighbors[i];
        switch (i)
        {
            case 0:
                return new List<int> { -1, 0 };
            case 1:
                return new List<int> { 1, 0 };
            case 2:
                return new List<int> { 0, -1 };
            case 3:
                return new List<int> { 0, 1 };
            case 4:
                return new List<int> { -1, -1 };
            case 5:
                return new List<int> { -1, 1 };
            case 6:
                return new List<int> { 1, -1 };
            case 7:
                return new List<int> { 1, 1 };
            default:
                return null;

        }
    }

    public int GetNeighbors()
    {
        int i = 0;
        foreach (Node node in neighbors)
        {
            if (node != null) i++;
        }
        return i;
    }

    public Node[] GetAllNeighbors()
    {
        return neighbors;
    }

    public List<int> GetNeighborCosts()
    {
        List<int> nCosts = new List<int>();
        foreach (Node node in neighbors)
        {
            if (node != null)
                nCosts.Add(node.GetCost());
            else
                nCosts.Add(1000);
        }
        return nCosts;
    }

    public int GetNeighborsOfTile(List<int> tiles)
    {
        int i = 0;
        foreach(Node node in neighbors)
        {
            if(node != null && tiles.Contains(node.GetOwnedByTile()))
            {
                i++;
            }
        }
        return i;
    }

    public void SetDir(byte val)
    {
        dir = val;
    }

    public byte GetDir()
    {
        return dir;
    }

    public void SetOwnedByTile(int i)
    {
        ownedByTile = i;
    }

    public int GetOwnedByTile()
    {
        return ownedByTile;
    }
}
