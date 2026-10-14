using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowFieldTile
{
    private FlowFieldTile[] neighbors;
    private List<List<Node>> portals; //new List<List<Node>>;
    private int[] rangeX;
    private int[] rangeY;
    private List<Node> nodes;
    //private Dictionary<int, int> pathFromPortal;
    private List<string> pathFromPortal;

    public FlowFieldTile()
    {
        nodes = new List<Node>();
        neighbors = new FlowFieldTile[4];
        portals = new List<List<Node>>(4);
        //existsWay = new List<bool>(6);
        //existsWay = Enumerable.Repeat(false, 6).ToList();
        //pathFromPortal = new Dictionary<int, int>();
        pathFromPortal = new List<string>(4);
        for (int i = 0; i < 4; i++)
        {
            portals.Add(new List<Node>());
            pathFromPortal.Add("");
        }
        rangeX = new int[2];
        rangeY = new int[2];
    }

    public void SetNeighbor(FlowFieldTile tile, int i)
    {
        neighbors[i] = tile;
    }

    public FlowFieldTile GetNeighbor(int i)
    {
        return neighbors[i];
    }

    public FlowFieldTile[] GetNeighbors()
    {
        return neighbors;
    }

    public void SetRangeX(int[] val)
    {
        rangeX = val;
    }

    public void SetRangeY(int[] val)
    {
        rangeY = val;
    }

    public void SetPortals(Node node, int i)
    {
        portals[i].Add(node);
        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(0, 0.3f), Color.red, 200);
    }

    public List<Node> GetPortal(int i)
    {
        return portals[i];
    }

    public List<List<Node>> GetPortals()
    {
        return portals;
    }

    private int IsNodePortal(Node node)
    {
        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i].Contains(node))
            {
                return i;
            }
        }
        return -1;
    }

    public int[] GetRangeX() { return rangeX; }

    public int[] GetRangeY() { return rangeY; }

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public List<Node> GetNodes()
    {
        return nodes;
    }

    public void SetPortalPath() //finds paths from one portal to another
    {
        ResetTileCost();
        for(int i = 0; i < pathFromPortal.Count; i++)
        {
            pathFromPortal[i] = "";
        }

        for (int i = 0; i < portals.Count; i++)
        {
            foreach (Node node in portals[i])
            {
                if (pathFromPortal[i].Length < 3)
                {
                    List<int> otherPortals = new List<int>() { 1, 2, 3, 4 }; //.Where(x => x != i).ToList()
                    FindExit(node, otherPortals, i);
                }
            }
        }
        foreach (string s in pathFromPortal)
        {
            foreach (char c in s)
            {
                Debug.DrawLine(portals[pathFromPortal.IndexOf(s)][0].GetPos(), portals[int.Parse(c.ToString())].First().GetPos(), Color.green, 15);
            }
        }
    }

    private void FindExit(Node argNode, List<int> otherPortals, int id)
    {
        Queue<Node> openList = new Queue<Node>();
        List<Node> visitedNodes = new List<Node>();

        openList.Enqueue(argNode);

        while (openList.Count > 0)
        {
            Node node = openList.Dequeue();
            if (node != null)
            {
                int portalId = IsNodePortal(node);
                if (otherPortals.Contains(portalId) && !pathFromPortal[id].Contains(portalId.ToString()))
                {
                    pathFromPortal[id] += portalId.ToString();
                    pathFromPortal[portalId] += id.ToString();
                }
            }

            foreach (Node next in node.GetAllNeighbors())
            {
                if (next != null && !visitedNodes.Contains(next) && node.GetCost() != ushort.MaxValue && node.GetOwnedByTile() == argNode.GetOwnedByTile())
                {
                    openList.Enqueue(next);
                    visitedNodes.Add(next);
                }
            }
        }
    }

    private void ResetTileCost()
    {
        GridGraph gg = AstarPath.active.data.gridGraph;

        foreach (Node node in GetNodes())
        {
            float x = node.GetPos().x;
            float y = node.GetPos().y;
            x += 53.9f;
            y += 53.9f;
            float xRem = x / 0.7f;
            float yRem = y / 0.7f;
            if (gg.nodes[(int)Math.Round(yRem) * gg.width + (int)Math.Round(xRem)].Walkable == true)
            {
                node.SetCost(1000);
            }
            else
            {
                node.SetCost(ushort.MaxValue);
            }
        }
    }

    public void FixPortals() //fixes portal nodes so there are only essential ones
    {
        List<int> newPortal = new List<int>();
        for (int i = 0; i < portals.Count; i++)
        {
            newPortal = new List<int>();
            int portalNumber = 0;
            for (int j = 0; j < portals[i].Count; j++)
            {
                if (portals[i][j].GetCost() == ushort.MaxValue || j == portals[i].Count - 1)
                {
                    newPortal.Add((int)MathF.Ceiling((portalNumber + j) / 2));
                    portalNumber = j;
                    portals[i][j] = null;
                }
            }

            for (int j = 0; j < portals[i].Count; j++)
            {
                if (!newPortal.Contains(j) || j == portals[i].Count - 1)
                {
                    portals[i][j] = null;
                }
            }
            portals[i] = portals[i].Where(x => x != null).ToList();
        }
    }

    public List<int> FindAvailablePortals(Node startNode)
    {
        Queue<Node> openList = new Queue<Node>();
        List<Node> nodesVisited = new List<Node>();

        openList.Enqueue(startNode);

        List<int> availablePortals = new List<int>();

        while (openList.Count > 0)
        {
            Node node = openList.Dequeue();
            int portalId = IsNodePortal(node);
            if (portalId != -1 && !availablePortals.Contains(portalId))
            {
                availablePortals.Add(portalId);
            }

            foreach (Node next in node.GetAllNeighbors())
            {
                if (next != null && !nodesVisited.Contains(next) && next.GetOwnedByTile() == startNode.GetOwnedByTile() && next.GetCost() != ushort.MaxValue)
                {
                    openList.Enqueue(next);
                    nodesVisited.Add(next);
                }
            }
        }
        return availablePortals;
    }

    public bool FindWayInsideTile(Node startNode, Node endNode) //tries to find a way to the end within the tile
    {
        Queue<Node> openList = new Queue<Node>();
        List<Node> nodesVisited = new List<Node>();

        openList.Enqueue(startNode);

        List<int> availablePortals = new List<int>();

        while (openList.Count > 0)
        {
            Node node = openList.Dequeue();
            int portalId = IsNodePortal(node);
            if (node == endNode)
            {
                return true;
            }

            foreach (Node next in node.GetAllNeighbors())
            {
                if (next != null && !nodesVisited.Contains(next) && next.GetOwnedByTile() == startNode.GetOwnedByTile() && next.GetCost() != ushort.MaxValue)
                {
                    openList.Enqueue(next);
                    nodesVisited.Add(next);
                }
            }
        }
        return false;
    }

    public List<int> AvailablePortalsFromPortal(int i)
    {
        string s = pathFromPortal[i];
        List<int> availablePortals = new List<int>();
        foreach (char c in s)
        {
            availablePortals.Add(int.Parse(c.ToString()));
        }
        return availablePortals;
    }

    public int GetPortalUsingNeighbor(int i)
    {
        int result = 0;
        switch (result)
        {
            case 0:
                result = 1;
                break;
            case 1:
                result = 0;
                break;
            case 2:
                result = 3;
                break;
            case 3:
                result = 2;
                break;
        }
        return result;
    }

    public void ResetPortals()
    {
        foreach(List<Node> portal in portals)
        {
            portal.Clear();
        }

        //Debug.Log(((GetRangeX()[0] * 0.7f) - 53.9f) + " " + ((GetRangeX()[1] * 0.7f) - 53.9f));
        foreach (Node node in nodes)
        {
            if (node.GetAllNeighbors().Where(x => x != null && x.GetOwnedByTile() != node.GetOwnedByTile()).Count() > 0)
            {
                //Debug.Log($"{Math.Round(node.GetPos().x, 2)} {Math.Round(node.GetPos().y, 2)}");

                bool isLeft = Math.Round((GetRangeX()[0] * 0.7f) - 53.9f, 2) == Math.Round(node.GetPos().x, 2);
                bool isRight = Math.Round((GetRangeX()[1] * 0.7f) - 53.9f, 2) == Math.Round(node.GetPos().x, 2);
                bool isDown = Math.Round((GetRangeY()[0] * 0.7f) - 53.9f, 2) == Math.Round(node.GetPos().y, 2);
                bool isUp = Math.Round((GetRangeY()[1] * 0.7f) - 53.9f, 2) == Math.Round(node.GetPos().y, 2);
                if (isLeft)
                {
                    portals[2].Add(node);
                }
                else if (isRight)
                {
                    portals[3].Add(node);
                }
                if (isUp)
                {
                    portals[1].Add(node);
                }
                else if (isDown)
                {
                    portals[0].Add(node);
                }
            }
        }
    }

    public void GetPaths()
    {
        foreach(string s in pathFromPortal)
        {
            Debug.Log(s);
        }
    }
}