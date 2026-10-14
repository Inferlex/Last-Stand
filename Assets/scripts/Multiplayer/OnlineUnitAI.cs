using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Assets.Src.Units;
using Unity.Netcode;

public class OnlineUnitAI : NetworkBehaviour
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
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    public void StartNewPath() => seeker.StartPath(rb.position, target.position, OnPathComplete);

    void OnPathComplete(Path p)
    {
        if (!p.error) //if it doesnt end with error
        {
            path = p;
            CurrentWaypoint = 0;
            moving = false;
        }
    }

    void Update()
    {
        if (path == null) //does path exist?
            return;

        if (CurrentWaypoint >= path.vectorPath.Count) //end of path
        {
            //reachedEndOfpath = true;
            if (gameObject.GetComponent<Unit>())
            {
                gameObject.GetComponent<Unit>().MovementStop();
            }
            else
            {
                //gameObject.GetComponent<OnlineUnit>().MovementStop();
            }
            rb.velocity = Vector2.zero;
            return;
        }
        else
        {
            //reachedEndOfpath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[CurrentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[CurrentWaypoint]);

        if (distance < 0.25f)
        {
            CurrentWaypoint++;
        }

        if (rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
}
