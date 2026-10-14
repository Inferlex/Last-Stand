using Assets.Src.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowFieldGraphManager : MonoBehaviour
{
    private List<Node> graphNodes = new List<Node>();
    private List<int> tileIds = new List<int>();
    private int unitsCount;
    private List<Unit> unitsDone = new List<Unit>();
    private Vector2 movePosition;
    private Unit leader;
    private Target target = null; //used for attacking
    private List<Unit> units = new List<Unit>();
    private int countdown;
    //private FlowFieldGraph ff;

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

    public void IncrementUnitsCount()
    {
        unitsCount++;
    }

    public Node GetNodeAtPosition(Vector2 pos)
    {
        float off = 0.35f;
        List<Node> list = graphNodes.Where(x => (x.GetPos().x - off < pos.x && x.GetPos().x + off > pos.x) && (x.GetPos().y - off < pos.y && x.GetPos().y + off > pos.y)).ToList();
        if (list == null || list.Count == 0)
        {
            Debug.Log("got out of bouds " + list.Count);
            return null;
        }
        return list[0];
    }

    public void PathDone(Unit unit)
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

    public bool FindUnit(Unit unit)
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

    public void SetLeader(List<Unit> units)
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
        foreach (Unit u in units)
        {
            avgPos += (Vector2)u.transform.position;
        }
        avgPos /= units.Count;
        float min = 1000;
        int unitId = 0;
        foreach (Unit u in units)
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

    public void SetLeader(Unit unit)
    {
        leader = unit;
    }

    public Unit GetLeader()
    {
        return leader;
    }

    public void SetTarget(Target t) => target = t;

    public Target GetTarget() => target;

    public void SetAttack(Target enemy, List<Unit> attackingUnits)
    {
        target = enemy;
        units = attackingUnits;
    }

    public void UpdateGraph()
    {
        countdown++;
        if(countdown == units.Count)
        {
            //FlowFieldGraphManager gm = ff.SetNodePoint(target.transform.position, units);
            FlowFieldGraphManager gm = FlowFieldGraph.Instance.SetNodePoint(target.transform.position, units);
            //gm.SetFf(ff);
            gm.SetAttack(target, units);
            gm.SetUnitsCount(units.Count);
            gm.SetLeader(units);
            foreach (Unit u in units)
            {
                u.SetFlowPathAttack(gm);
                //u.SetAttackQueued(target);
            }
            StartCoroutine(DestroyGraph());
        }
    }

    private IEnumerator DestroyGraph()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(this);
    }
}
