using Assets.Src.Units;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowFieldGraph : MonoBehaviour
{
    private static FlowFieldGraph instance;
    private List<List<Node>> graphNodes;
    private int graphArea;

    public static FlowFieldGraph active;

    //[SerializeField] FlowFieldTile tilePrefab;
    private List<FlowFieldTile> tiles;
    [SerializeField] FlowFieldGraphManager gm;

    private void Start() //inialize starting graph
    {
        instance = this;
        tiles = new List<FlowFieldTile>();
        for (int i = 0; i < 25; i++)
        {
            tiles.Add(new FlowFieldTile());
        }

        graphArea = AstarPath.active.data.gridGraph.width; //map is square, so we only need width
        graphNodes = new List<List<Node>>();
        float border = -53.9f;
        float x = border;
        float y = border;
        int tileArea = (graphArea / 5);

        for (int i = 0; i < 5; i++) //setting range for tiles
        {
            for (int j = 0; j < 5; j++) //setting range for tiles
            {
                tiles[(i * 5) + j].SetRangeY(new int[] { i * 31, (i + 1) * 31 - 1 });
                tiles[(i * 5) + j].SetRangeX(new int[] { j * 31, (j + 1) * 31 - 1 });
            }
        }

        for (int i = 0; i < graphArea; i++) //making nodes
        {
            graphNodes.Add(new List<Node>());
            for (int j = 0; j < graphArea; j++)
            {
                graphNodes[i].Add(new Node());
                Node node = graphNodes[i][j];
                node.SetPos(new Vector2(x, y));

                int ii = (int)Mathf.Floor(i / tileArea);
                int jj = (int)Mathf.Floor(j / tileArea);
                int id = ii * (int)MathF.Sqrt(tiles.Count) + jj;

                node.SetOwnedByTile(id);
                tiles[id].AddNode(node);
                if (i == tiles[id].GetRangeY()[0])
                {
                    //Debug.Log("Y0 " + (tiles[id].GetRangeX()[0] + tiles[id].GetRangeX()[1] / 2));
                    tiles[id].SetPortals(node, 0);
                }
                if (i == tiles[id].GetRangeY()[1])
                {
                    //Debug.Log("Y0 " + (tiles[id].GetRangeX()[0] + tiles[id].GetRangeX()[1] / 2));
                    tiles[id].SetPortals(node, 1);
                }
                if (j == tiles[id].GetRangeX()[0])
                {
                    //Debug.Log("X0 " + (tiles[id].GetRangeY()[0] + tiles[id].GetRangeY()[1] / 2));
                    tiles[id].SetPortals(node, 2);
                }
                if (j == tiles[id].GetRangeX()[1])
                {
                    //Debug.Log("X1 " + (tiles[id].GetRangeY()[0] + tiles[id].GetRangeY()[1] / 2));
                    tiles[id].SetPortals(node, 3);
                }
                x += 0.7f;
            }
            y += 0.7f;
            x = border;
        }

        for (int i = 0; i < 25; i++) //setting tile neighbors
        {
            if (i != 0 && 0 != i % 5)
            {
                tiles[i].SetNeighbor(tiles[i - 1], 2);
            }
            if (i == 0 || 4 != i % 5 && i != 24)
            {
                tiles[i].SetNeighbor(tiles[i + 1], 3);
            }
            if (i > 4)
            {
                tiles[i].SetNeighbor(tiles[i - 5], 0);
            }
            if (i < 20)
            {
                tiles[i].SetNeighbor(tiles[i + 5], 1);
            }
        }

        for (int i = 0; i < graphArea; i++) //setting node neighbors
        {
            for (int j = 0; j < graphArea; j++)
            {
                if (i != 0)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i - 1][j], 0); //down
                }
                if (i != graphArea - 1)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i + 1][j], 1); //up
                }
                if (j != 0)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i][j - 1], 2); //left
                }
                if (j != graphArea - 1)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i][j + 1], 3); //right
                }
                if (i != 0 && j != 0)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i - 1][j - 1], 4); //down left
                }
                if (i != 0 && j != graphArea - 1)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i - 1][j + 1], 5); //down right
                }
                if (i != graphArea - 1 && j != 0)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i + 1][j - 1], 6); //up left
                }
                if (i != graphArea - 1 && j != graphArea - 1)
                {
                    graphNodes[i][j].SetNeighbor(graphNodes[i + 1][j + 1], 7); //up right
                }
            }
        }

        ResetNodes();
        foreach (FlowFieldTile tile in tiles) //fixing portals
        {
            tile.FixPortals();
        }

        foreach (FlowFieldTile tile in tiles)
        {
            tile.SetPortalPath();
        }
    }

    public FlowFieldGraphManager SetNodePoint(Vector2 pos, List<Unit> units) //closest node to pos will be used as a goal position
    {
        List<int> tileIds = new List<int>();
        List<Node> unitNodes = new List<Node>();
        foreach (Unit u in units)
        {
            unitNodes.Add(GetNodeAtPosition(u.gameObject.transform.position));
            int id = unitNodes.Last().GetOwnedByTile();
            if (!tileIds.Contains(id))
            {
                tileIds.Add(id);
            }
        }

        foreach (List<Node> nodeRow in graphNodes)
        {
            foreach (Node node in nodeRow)
            {
                if (node.GetPos().x - (node.GetArea() / 2) < pos.x && pos.x < node.GetPos().x + (node.GetArea() / 2))
                {

                    if (node.GetPos().y - (node.GetArea() / 2) < pos.y && pos.y < node.GetPos().y + (node.GetArea() / 2))
                    {
                        List<FlowFieldTile> graphTiles = TileSearch(tileIds, node.GetOwnedByTile(), unitNodes, node);

                        if (graphTiles == null)
                        {
                            return null;
                        }

                        List<int> ids = new List<int>();

                        foreach (FlowFieldTile tile in graphTiles)
                        {
                            ids.Add(tiles.IndexOf(tile));
                        }
                        CreateFlowField(graphNodes.IndexOf(nodeRow), nodeRow.IndexOf(node), ids);

                        FlowFieldGraphManager newGraph = Instantiate(gm, transform.position, Quaternion.identity);
                        node.SetDir(0);
                        foreach (FlowFieldTile tile in graphTiles)
                        {
                            newGraph.AddNodes(tile.GetNodes());
                            newGraph.AddId(tiles.IndexOf(tile));
                        }
                        newGraph.SetMovePosition(node.GetPos());
                        return newGraph;
                    }
                }
                else if (node.GetPos().x + (node.GetArea() / 2) > pos.x)
                {
                    break;
                }
            }
        }
        return null;
    }

    public FlowFieldGraphManager SetNodePoint(Node targetNode, List<Unit> units) //closest node to pos will be used as a goal position
    {
        List<int> tileIds = new List<int>();
        List<Node> unitNodes = new List<Node>();
        foreach (Unit u in units)
        {
            unitNodes.Add(GetNodeAtPosition(u.gameObject.transform.position));
            int id = unitNodes.Last().GetOwnedByTile();
            if (!tileIds.Contains(id))
            {
                tileIds.Add(id);
            }
        }

        List<FlowFieldTile> graphTiles = TileSearch(tileIds, targetNode.GetOwnedByTile(), unitNodes, targetNode);
        if (graphTiles == null)
        {
            return null;
        }
        List<int> ids = new List<int>();

        foreach (FlowFieldTile tile in graphTiles)
        {
            ids.Add(tiles.IndexOf(tile));
        }

        List<Node> nodeRow = new List<Node>();
        foreach (List<Node> nr in graphNodes)
        {
            if (nr.Contains(targetNode))
            {
                nodeRow = nr;
            }
        }

        CreateFlowField(graphNodes.IndexOf(nodeRow), nodeRow.IndexOf(targetNode), ids);

        FlowFieldGraphManager newGraph = Instantiate(gm, transform.position, Quaternion.identity);
        targetNode.SetDir(0);
        foreach (FlowFieldTile tile in graphTiles)
        {
            newGraph.AddNodes(tile.GetNodes());
            newGraph.AddId(tiles.IndexOf(tile));
        }
        newGraph.SetMovePosition(targetNode.GetPos());
        return newGraph;
    }

    public Node GetNodeAtPosition(Vector2 pos)
    {
        Node node = graphNodes[graphArea / 2][graphArea / 2];
        float off = node.GetArea() / 2;
        while (true)
        {
            Vector2 nodePos = node.GetPos();
            bool xInPos = (nodePos.x - off < pos.x && nodePos.x + off > pos.x);
            bool yInPos = (nodePos.y - off < pos.y && nodePos.y + off > pos.y);

            if (xInPos && yInPos)
            {
                break;
            }

            bool xx = (pos.x > nodePos.x);
            bool yy = (pos.y > nodePos.y);
            if (xInPos && yy)
            {
                //Debug.Log("go up");
                node = node.GetNeighbor(1);
            }
            else if (xInPos && !yy)
            {
                //Debug.Log("go down");
                node = node.GetNeighbor(0);
            }
            else if (yInPos && xx)
            {
                //Debug.Log("go right");
                node = node.GetNeighbor(3);
            }
            else if (yInPos && !xx)
            {
                //Debug.Log("go left");
                node = node.GetNeighbor(2);
            }
            else if (xx && yy)
            {
                //Debug.Log("go up right");
                node = node.GetNeighbor(7);
            }
            else if (!xx && yy)
            {
                //Debug.Log("go up left");
                node = node.GetNeighbor(6);
            }
            else if (!xx && !yy)
            {
                //Debug.Log("go down left");
                node = node.GetNeighbor(4);
            }
            else if (xx && !yy)
            {
                //Debug.Log("go down right");
                node = node.GetNeighbor(5);
            }
        }
        return node;
    }

    private List<FlowFieldTile> TileSearch(List<int> startTiles, int end, List<Node> nodes, Node exitNode)
    {
        List<FlowFieldTile> path = new List<FlowFieldTile>();

        List<int> exitPortals = tiles[end].FindAvailablePortals(exitNode); //cant use node, its for unit
        for (int i = 0; i < nodes.Count; i++) //fix this, so it works when theres more units in the tile (nodes.count > starttiles.count)
        {
            if (path.Contains(tiles[nodes[i].GetOwnedByTile()])) //if the tile is already in path, skip the node
            {
                continue;
            }
            Queue<FlowFieldTile> openList = new Queue<FlowFieldTile>();
            Dictionary<int, int> cameFrom = new Dictionary<int, int>();
            List<int> availablePortals = tiles[nodes[i].GetOwnedByTile()].FindAvailablePortals(nodes[i]); //we need to know portals available to the unit
            FlowFieldTile current = tiles[end];

            if (cameFrom.ContainsKey(nodes[i].GetOwnedByTile()))
            {
                if (!path.Contains(tiles[nodes[i].GetOwnedByTile()]))
                {
                    path.Add(tiles[nodes[i].GetOwnedByTile()]);
                }
                continue;
            }

            cameFrom.Add(nodes[i].GetOwnedByTile(), -1);
            openList.Enqueue(tiles[nodes[i].GetOwnedByTile()]);

            while (openList.Count > 0)
            {
                FlowFieldTile tile = openList.Dequeue();

                if (cameFrom[tiles.IndexOf(tile)] != -1) //tile != tiles[startTiles[i]]
                {
                    FlowFieldTile prevTile = tiles[cameFrom[tiles.IndexOf(tile)]];
                    availablePortals.Clear();
                    foreach (int val in tile.AvailablePortalsFromPortal(tile.GetNeighbors().ToList().IndexOf(prevTile)))
                    {
                        availablePortals.Add(val);
                    }
                }

                if (tiles.IndexOf(tile) == end)
                {
                    if (startTiles.Contains(end) && cameFrom[tiles.IndexOf(tile)] == -1)
                    {
                        if (tile.FindWayInsideTile(nodes[i], exitNode))
                        {
                            break;
                        }
                        else if (cameFrom[tiles.IndexOf(tile)] == -1)
                        {
                            cameFrom.Remove(tiles.IndexOf(tile));
                            openList.Clear();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (cameFrom.Count == 0)
                {
                    List<int> portals = availablePortals.Intersect(exitPortals).ToList();
                    foreach (FlowFieldTile next in tile.GetNeighbors())
                    {
                        if (next != null)
                        {
                            if (portals.Contains(tile.GetNeighbors().ToList().IndexOf(next)))
                            {
                                openList.Enqueue(next);
                                cameFrom.Add(tiles.IndexOf(next), -1);
    
                            }
                        }
                    }         
                }
                else
                {
                    foreach (FlowFieldTile next in tile.GetNeighbors())
                    {
                        if (next != null)
                        {
                            //int tileIndex = next.GetNeighbors().ToList().IndexOf(tile); //we dont use it?
                            if (!cameFrom.ContainsKey(tiles.IndexOf(next)) && availablePortals.Contains(tile.GetNeighbors().ToList().IndexOf(next)))
                            {
                                if (next == tiles[end] && !exitPortals.Contains(next.GetNeighbors().ToList().IndexOf(tile))) //if we dont have a clear path to the goal, dont use node
                                {
                                    continue;
                                }
                                openList.Enqueue(next);

                                cameFrom.Add(tiles.IndexOf(next), tiles.IndexOf(tile));

                            }
                        }
                    }
                }
            }
            try
            {
                while (true) //current != tiles[startTiles[i]]
                {
                    path.Add(current);
                    if (!cameFrom.ContainsKey(tiles.IndexOf(current)))
                    {
                        Debug.LogError("path doesnt exist");
                        return null;
                    }
                    if (cameFrom[tiles.IndexOf(current)] == -1)
                    {
                        break;
                    }
                    current = tiles[cameFrom[tiles.IndexOf(current)]];
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
        return path;
    }
     

    private void CreateFlowField(int i, int j, List<int> tileIds)
    {
        ResetFieldForTiles(tileIds);
        graphNodes[i][j].SetCost(0);

        Queue<List<int>> queue = new Queue<List<int>>();
        queue.Enqueue(new List<int> { i, j, graphNodes[i][j].GetCost() }); //coordinates of node and its cost

        while (queue.Count > 0)
        {
            List<int> l = queue.Dequeue();
            Node node = graphNodes[l[0]][l[1]];
            if (l[2] > node.GetCost())
                continue;

            for (int k = 0; k < 8; k++) // we can use 4 or 8
            {
                Node neighbor = node.GetNeighbor(k);
                if (neighbor != null && tileIds.Contains(neighbor.GetOwnedByTile()))
                {
                    if (node.GetCost() + 1 < neighbor.GetCost() && neighbor.GetCost() != ushort.MaxValue)
                    {
                        ushort u = (ushort)(node.GetCost() + 1);
                        neighbor.SetCost(u);
                        queue.Enqueue(new List<int> { node.GetNeighborPos(k)[0] + l[0], node.GetNeighborPos(k)[1] + l[1], u });
                    }
                }
            }
        }
        SetDirections(tileIds);
    }

    private void SetDirections(List<int> tileIds)
    {
        foreach (int i in tileIds)
        {
            foreach (Node node in tiles[i].GetNodes())
            {
                if (node.GetCost() != 0) //node.GetCost() != ushort.MaxValue && 
                {
                    List<int> values = node.GetNeighborCosts();
                    int min = int.MaxValue;
                    int forValue = 8;
                    if (node.GetNeighborsOfTile(tileIds) < 8)
                    {
                        forValue = 4;
                    }

                    for (int j = 0; j < forValue; j++)
                    {
                        Node neighbor = node.GetNeighbor(j);
                        if (neighbor != null && tileIds.Contains(neighbor.GetOwnedByTile()))
                        {
                            if (values[j] < min)
                            {
                                min = values[j];
                            }
                        }
                    }

                    if (values[0] == min)
                    {
                        node.SetDir(1); //down
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(0, -0.3f), Color.red, 20);
                    }
                    else if (values[1] == min)
                    {
                        node.SetDir(5); //up
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(0, 0.3f), Color.red, 20);
                    }
                    else if (values[2] == min)
                    {
                        node.SetDir(3); //left
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(-0.3f, 0), Color.red, 20);
                    }
                    else if (values[3] == min)
                    {
                        node.SetDir(7); //right
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(0.3f, 0), Color.red, 20);
                    }
                    else if (values[4] == min)
                    {
                        node.SetDir(2); //down-left
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(-0.3f, -0.3f), Color.red, 20);
                    }
                    else if (values[5] == min)
                    {
                        node.SetDir(8);//down-right
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(0.3f, -0.3f), Color.red, 20);
                    }
                    else if (values[6] == min)
                    {
                        node.SetDir(4); //up-left
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(-0.3f, 0.3f), Color.red, 20);
                    }
                    else if (values[7] == min)
                    {
                        node.SetDir(6); //up-right
                        Debug.DrawLine(node.GetPos(), node.GetPos() + new Vector2(0.3f, 0.3f), Color.red, 20);
                    }
                }
            }
        }
    }

    private void ResetFieldForTiles(List<int> ids)
    {
        GridGraph gg = AstarPath.active.data.gridGraph;
        foreach (int i in ids)
        {
            foreach (Node node in tiles[i].GetNodes())
            {
                float x = node.GetPos().x;
                float y = node.GetPos().y;
                x += 53.9f;
                y += 53.9f;
                float xRem = x / 0.7f;
                float yRem = y / 0.7f;
                if (gg.nodes[(int)Math.Round(yRem) * gg.width + (int)Math.Round(xRem)].Walkable == true)
                {
                    //Debug.Log(node.GetOwnedByTile());
                    node.SetCost(1000);
                }
                else
                {
                    node.SetCost(ushort.MaxValue);
                }
            }
        }
    }

    public static FlowFieldGraph Instance => instance;

    private void ResetNodes()
    {
        GridGraph gg = AstarPath.active.data.gridGraph;
        foreach (List<Node> nodeRow in graphNodes)
        {
            foreach (Node node in nodeRow)
            {
                float x = node.GetPos().x;
                float y = node.GetPos().y;
                x += 53.9f;
                y += 53.9f;
                float xRem = x / 0.7f;
                float yRem = y / 0.7f;
                if (gg.nodes[(int)Math.Round(yRem) * gg.width + (int)Math.Round(xRem)].Walkable == true)
                {
                    //Debug.Log(node.GetOwnedByTile());
                    node.SetCost(1000);
                }
                else
                {
                    node.SetCost(ushort.MaxValue);
                }
            }
        }
    }

    public void UpdateTilePortals(Building building)
    {
        FlowFieldTile tile = GetTileFromPos(building.transform.position);
        tile.ResetPortals();
        tile.FixPortals();
        tile.SetPortalPath();
    }

    public FlowFieldTile GetTileFromPos(Vector2 pos)
    {
        FlowFieldTile tile = tiles[12];
        while (true)
        {
            float minX = (tile.GetRangeX()[0] * 0.7f) - 53.9f;
            float maxX = (tile.GetRangeX()[0] * 0.7f) - 53.9f + 21.7f;
            float minY = (tile.GetRangeY()[0] * 0.7f) - 53.9f;
            float maxY = (tile.GetRangeY()[0] * 0.7f) - 53.9f + 21.7f;
            if (pos.x > minX && pos.x < maxX)
            {
                if (pos.y > (tile.GetRangeY()[0] * 0.7f) - 53.9f && pos.y < (tile.GetRangeY()[0] * 0.7f) - 53.9f + 21.7f)
                {
                    return tile;
                }
            }
            if (pos.x < minX)
            {
                tile = tile.GetNeighbors()[2];
            }
            else if (pos.y < minY)
            {
                tile = tile.GetNeighbors()[0];
            }
            else if (pos.x > maxX)
            {
                tile = tile.GetNeighbors()[3];
            }
            else if (pos.y > maxY)
            {
                tile = tile.GetNeighbors()[1];
            }
        }
    }

    public Node GetBuildingNode(Target target)
    {
        Queue<Node> nodes = new Queue<Node>();
        nodes.Enqueue(GetNodeAtPosition(target.transform.position));
        while (true)
        {
            Node node = nodes.Dequeue();
            if (node.GetCost() != ushort.MaxValue)
            {
                return node;
            }
            foreach (Node next in node.GetAllNeighbors())
            {
                if (next != null)
                {
                    nodes.Enqueue(next);
                }
            }
        }
    }
}
