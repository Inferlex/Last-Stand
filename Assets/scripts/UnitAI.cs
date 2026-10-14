using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Assets.Src.Units;
using Unity.Netcode.Components;
using System.Linq;

public class UnitAI : MonoBehaviour
{
    public Transform target;

    public float speed = 200f;
    public float nextWaipointDistance = 3f;

    Path path;
    int CurrentWaypoint = 0;
    //bool reachedEndOfpath = false;
    public bool moving = false;
    //private Unit unit;

    Seeker seeker; //finds path
    Rigidbody2D rb;

    private bool isCalculating;

    private Vector2 offset = Vector2.zero;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
  
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone() && moving)
        {           
            isCalculating = true;
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    public void StartNewPath()
    {
        isCalculating = true;
        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p) //called when path is calculated
    {
        /*if (!p.error) //if it doesnt end with error
        {
            path = p;
            CurrentWaypoint = 0;
            moving = false;
        }
        if (Vector2.Distance(target.position, path.vectorPath.Last()) > 1f)
        {
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            Vector2 force = direction * speed * Time.deltaTime;
            transform.position += (Vector3)force / 100;
            StartNewPath();
        }
        GetComponent<Animator>().SetBool("Running", true);
        if (GetComponent<Unit>())
        {
            GetComponent<Unit>().SetIsMoving(true);
        }
        else
        {
            GetComponent<OnlineUnit>().SetIsMoving(true);
        }
        isCalculating = false;*/
        PathCompleted(p, Vector2.zero);
    }

    void PathCompleted(Path p, Vector2 offset) //uses offset
    {
        this.offset = offset;
        if (!p.error) //if it doesnt end with error
        {
            path = p;
            CurrentWaypoint = 0;
            moving = false;
        }
        GetComponent<Animator>().SetBool("Running", true);
        if (GetComponent<Enemy>())
        {
            return;
        }
        if (Vector2.Distance(target.position, path.vectorPath.Last() + (Vector3)offset) > 1f)
        {
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            Vector2 force = direction * speed * Time.deltaTime;
            transform.position += (Vector3)force / 100;
            StartNewPath();
        }
        if (GetComponent<Unit>())
        {
            GetComponent<Unit>().SetIsMoving(true);
        }
        else
        {
            GetComponent<OnlineUnit>().SetIsMoving(true);
        }
        isCalculating = false;
    }

    void Update()
    {
        if (path == null)//does path exist?
        {
            return;
        }

        if (CurrentWaypoint >= path.vectorPath.Count) //end of path
        {
            //path = null; //should be here, just testing the group ai
            if (gameObject.GetComponent<Unit>())
            {
                gameObject.GetComponent<Unit>().MovementStop();
            }
            else
            {
                GetComponent<OnlineUnit>().MovementStop();
            }
            return;
        }

        Vector2 direction = (((Vector2)path.vectorPath[CurrentWaypoint]+offset) - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        if (GetComponent<Enemy>())
        {
            rb.AddForce(force);
        }
        //transform.position += new Vector3(force.x, force.y, 0) / 200;

        float distance = Vector2.Distance(rb.position, ((Vector2)path.vectorPath[CurrentWaypoint] + offset));

        if (distance < 0.25f)
        {
            CurrentWaypoint++;
        }

        if (target.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (target.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    public void SetPath(Vector3 vector)
    {
        moving = true;
        target.position = vector;
    }

    public Vector2 GetPath() { return target.position; }

    public bool GetIsCalculating() => isCalculating;

    public void SetPathToFollow(Path p, Vector3 offset)
    {
        target.transform.position = p.vectorPath.Last() + offset;
        if (GetComponent<Unit>())
        {
            GetComponent<Unit>().SetAttackQueued(null);
        }
        else if(GetComponent<OnlineUnit>())
        {
            GetComponent<OnlineUnit>().SetAttackQueued(null);
        }
            PathCompleted(p, offset);
    }
}
