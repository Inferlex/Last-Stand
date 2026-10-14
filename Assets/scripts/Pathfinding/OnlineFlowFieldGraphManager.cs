using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class OnlineFlowFieldGraphManager : MonoBehaviour
{
    private List<Node> graphNodes = new List<Node>();
    private List<int> tileIds = new List<int>();
    private int unitsCount;
    private List<OnlineUnit> unitsDone = new List<OnlineUnit>();
    private Vector2 movePosition;
    private OnlineUnit leader;
    private OnlineTarget target = null; //used for attacking
    private List<OnlineUnit> units = new List<OnlineUnit>();
    private int countdown;
    private FlowFieldGraph ff;

    public void AddNodes(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            graphNodes.Add(new Node(node.GetOwnedByTile(), node.GetPos(), node.GetDir(), node.GetCost()));
        }
    }

    public List<Node> GetNodes()
    {
        return graphNodes;
    }

    public void SetUnitsCount(int val)
    {
        unitsCount = val;
    }

    public Node GetNodeAtPosition(Vector2 pos)
    {
        float off = 0.35f;
        List<Node> list = graphNodes.Where(x => (x.GetPos().x - off < pos.x && x.GetPos().x + off > pos.x) && (x.GetPos().y - off < pos.y && x.GetPos().y + off > pos.y)).ToList();
        if (list == null || list.Count == 0)
        {
            Debug.Log("sumething is wrong " + list.Count);
            return null;
        }
        return list[0];
    }

    public void PathDone(OnlineUnit unit)
    {
        unitsCount--;
        if (unitsCount == 0)
        {
            Destroy(gameObject);
        }
        unitsDone.Add(unit);
    }

    public void AddId(int id)
    {
        tileIds.Add(id);
    }

    public bool FindUnit(OnlineUnit unit)
    {
        return unitsDone.Contains(unit);
    }

    public void SetMovePosition(Vector2 pos)
    {
        movePosition = pos;
    }

    public Vector2 GetMovePosition()
    {
        return movePosition;
    }

    public void SetLeader(List<OnlineUnit> units)
    {
        if (units.Count < 2)
        {
            if (units.Count == 1)
            {
                leader = units[0];
            }
            return;
        }
        Vector2 avgPos = new Vector2();
        foreach (OnlineUnit u in units)
        {
            avgPos += (Vector2)u.transform.position;
        }
        avgPos /= units.Count;
        float min = 1000;
        int unitId = 0;
        foreach (OnlineUnit u in units)
        {
            float distance = Vector2.Distance(u.transform.position, avgPos);
            if (distance < min)
            {
                min = distance;
                unitId = units.IndexOf(u);
            }
        }
        leader = units[unitId];
    }

    public void SetLeader(OnlineUnit unit)
    {
        leader = unit;
    }

    public OnlineUnit GetLeader()
    {
        return leader;
    }

    public void SetFf(FlowFieldGraph val) => ff = val;

    public void UpdateGraph()
    {
        countdown++;
        if (countdown == units.Count)
        {
            OnlineFlowFieldGraphManager gm = OnlineFlowFieldGraph.Instance.SetNodePoint(target.transform.position, units);
            //gm.SetAttack(target, units);
            gm.SetUnitsCount(units.Count);
            gm.SetLeader(units);
            foreach (OnlineUnit u in units)
            {
                u.SetFlowPath(gm);
                u.SetAttackQueued(target);
            }
            StartCoroutine(DestroyGraph());
        }
    }

    private IEnumerator DestroyGraph()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(this);
    }

    public void SetAttack(OnlineTarget enemy, List<OnlineUnit> attackingUnits)
    {
        target = enemy;
        units = attackingUnits;
    }
}
