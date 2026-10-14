using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Src.Units
{
    public class OnlineUnit : OnlineTarget
    {
        public float fireRate;
        public int damage;
        public float attackRange;
        UnitAI ai;
        public Transform dest;
        private bool cooldown = false;
        private bool isMoving = false;
        //private CircleCollider2D radius;
        //private bool isMoving = false;
        private List<OnlineUnit> enemiesInRange = new List<OnlineUnit>();
        public int capacityCost;
        public bool armorPiercing;
        public bool splash;
        public GameObject splashObject;
        public bool flying;
        private bool inflicted = false;
        private Animator animator;
        private OnlineHero hero; //used for commander
        public int xp;

        private OnlineTarget attackQueued = null;

        private bool cantMove = false;
        private OnlineFlowFieldGraphManager flowPath;
        private bool flowCooldown;
        private OnlineBoid boid;

        public void Awake()
        {
            ai = GetComponent<UnitAI>();
            dest.SetParent(null);
            ai.target = dest;
        }

        public new void Start()
        {
            base.Start();
            gameObject.transform.GetChild(0).GetComponent<OnlineUnitRadius>().SetUnit(this); //might get rid of this as we serialize the field

            gameObject.transform.GetChild(0).SetParent(null);

            animator = GetComponent<Animator>();

            if (transform.GetChild(transform.childCount - 1).GetComponent<OnlineBoid>() != null)
            {
                boid = transform.GetChild(transform.childCount - 1).GetComponent<OnlineBoid>();
            }
        }

        public void Update()
        {
            if (attackQueued) //&& !movedBefore
            {
                Attack(attackQueued);
            }
            if (!attackQueued && enemiesInRange.Count > 0) //no enemy locked but there are enemies close
            {
                attackQueued = enemiesInRange[0];
                enemiesInRange.Remove(enemiesInRange[0]);
            }
            if (flowPath)
            {
                FlowMove(transform.position);
            }
        }

        public void Attack(OnlineTarget target)
        {
            if (target.GetHp() <= 0) //gethp is in case the target is hero and dies (gets deactivated) and range so it wont chase the guy if they are out of range
            {
                attackQueued = null;
                return;
            }

            if (cantMove) return;
            if (target.GetComponent<OnlineUnit>() && target.GetComponent<OnlineUnit>().flying && attackRange < 2) //cannot attack flying enemies in close combat
            {
                GameEvents.current.ShowMessage("Cannot attack flying creature with melee unit/" + GetId());
                return;
            }
            if (!cooldown) //if unit is not on cooldown
            {
                var distance = Vector2.Distance(target.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.position);
                if (distance > attackRange)
                {
                    //MoveWhileAttack(Vector2.MoveTowards(transform.position, target.transform.position, distance - attackRange + 0.5f));
                    if (!flowCooldown && target.GetComponent<OnlineBuilding>() == null)
                    {
                        flowCooldown = true;
                        ResetFlowField(target.GetComponent<Collider2D>().ClosestPoint(transform.position));
                    }
                    attackQueued = target; // move cancels attackqueued
                }
                else
                {
                    cooldown = true;
                    //MoveWhileAttack(transform.position); //stops the unit
                    if (flowPath)
                    {
                        flowPath.PathDone(this);
                        if (boid)
                        {
                            boid.SetIsOn(false);
                        }
                        flowPath = null;
                    }
                    animator.SetBool("Running", false);
                    gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero; //stops the unit
                    if (IsServer)
                    {
                        AttackAnimationRpc(); //to synchronize the sounds
                    }

                    if (splash)
                    {
                        if (GetIsOwnedByPlayer())
                        {
                            CreateSplashAttackRpc(target.transform.position);
                        }
                    }
                    else
                    {
                        if (target)
                        {
                            if (gameObject.GetComponent<OnlineAcolyte>())
                            {
                                gameObject.GetComponent<OnlineAcolyte>().ShowAttack(target);
                            }
                            if (gameObject.GetComponent<OnlineArtificier>())
                            {
                                gameObject.GetComponent<OnlineArtificier>().ShowAttack(target);
                            }
                            if (hero == null)
                            {
                                target.GetComponent<OnlineTarget>().TakeDamage(damage, this, armorPiercing); //used for when hero kills an enemy to get xp
                            }
                            else
                            {
                                target.GetComponent<OnlineTarget>().TakeDamage(damage, hero, armorPiercing); //used for when hero kills an enemy to get xp
                            }

                        }
                    }
                    StartCoroutine(Wait(target));
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void CreateSplashAttackRpc(Vector2 pos)
        {
            GameObject splash = Instantiate(splashObject, pos, Quaternion.identity);
            splash.GetComponent<NetworkObject>().Spawn();
            splash.GetComponent<OnlineSplashDamage>().SetUnit(this);
            splash.GetComponent<OnlineSplashDamage>().Activate(damage, GetId());
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void AttackAnimationRpc()
        {
            GetComponent<AudioSource>().Play();
            animator.CrossFade("Attack", 0, 0);
        }

        public IEnumerator Wait(OnlineTarget target)
        {
            MovementStop();
            /*if (target.GetHp() > 0)
            {
                attackQueued = target;
            }*/
            yield return new WaitForSeconds(fireRate);
            cooldown = false;
        }

        public IEnumerator Wait()
        {
            yield return new WaitForSeconds(fireRate);
            cooldown = false;
        }


        public void Move(Vector2 pos)
        {
            if (cantMove) return;
            if (GetComponent<OnlineRepairUnit>())
            {
                GetComponent<OnlineRepairUnit>().SetRepairingBuilding(null);
            }
            if (flying)
            {
                /*Vector2 direction = (pos - GetComponent<Rigidbody2D>().position).normalized;
                Vector2 force = ai.speed * Time.deltaTime * direction;
                GetComponent<Rigidbody2D>().AddForce(force);*/
                Vector2 direction = (pos - GetComponent<Rigidbody2D>().position).normalized;
                Vector2 force = direction * ai.speed * Time.deltaTime;

                transform.position += new Vector3(force.x, force.y, 0) / 200;
            }
            else
            {
                attackQueued = null;
                GetComponent<UnitAI>().SetPath(new Vector3(pos.x, pos.y, 1f));
            }
        }

        public void MoveWhileAttack(Vector2 pos) //normal move but attackqueued is not set to null
        {
            if (cantMove) return;
            if (GetComponent<RepairUnit>())
            {
                GetComponent<RepairUnit>().SetRepairingBuilding(null);
            }
            if (flying)
            {
                Vector2 direction = (pos - GetComponent<Rigidbody2D>().position).normalized;
                Vector2 force = ai.speed * Time.deltaTime * direction;
                GetComponent<Rigidbody2D>().AddForce(force);
            }
            else
            {
                GetComponent<UnitAI>().SetPath(new Vector3(pos.x, pos.y, 1f));
            }
        }

        public void FlowMove(Vector2 unitPos)
        {
            Node node = flowPath.GetNodeAtPosition(unitPos);
            OnlineUnit leader = flowPath.GetLeader(); //we might also want to get closer to leader when we can
            if (leader == this && (Vector2.Distance(unitPos, leader.transform.position) > 2 || flowPath.FindUnit(leader)))
            {
                node = flowPath.GetNodeAtPosition(unitPos);
            }
            else if (leader != null && node != null && node.GetCost() != ushort.MaxValue)
            {
                node = flowPath.GetNodeAtPosition(leader.transform.position);
            }

            if (node == null)
            {
                return;
            }

            int dir = node.GetDir();
            Vector2 direction = Vector2.zero;
            Vector2 force = Vector2.zero;
            switch (dir)
            {
                case 0:
                    PathDone();
                    //GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    return;
                case 1:
                    direction = new Vector2(0, -1);
                    //Debug.Log("down");
                    break;
                case 2:
                    direction = new Vector2(-1, -1);
                    //Debug.Log("down left");
                    break;
                case 3:
                    direction = new Vector2(-1, 0);
                    //Debug.Log("left");
                    break;
                case 4:
                    direction = new Vector2(-1, 1);
                    //Debug.Log("up left");
                    break;
                case 5:
                    direction = new Vector2(0, 1);
                    //Debug.Log("up");
                    break;
                case 6:
                    direction = new Vector2(1, 1);
                    //Debug.Log("up right");
                    break;
                case 7:
                    direction = new Vector2(1, 0);
                    //Debug.Log("right");
                    break;
                case 8:
                    direction = new Vector2(1, -1);
                    //Debug.Log("down right");
                    break;
            }
            force = direction * ai.speed * Time.deltaTime;
            transform.position += new Vector3(force.x, force.y, 0) / 200;
            //GetComponent<Rigidbody2D>().AddForce(force);
            if (force.x > 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (force.x < 0)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        public void SetFlowPath(OnlineFlowFieldGraphManager tile)
        {
            if (GetComponent<OnlineRepairUnit>())
            {
                GetComponent<OnlineRepairUnit>().SetRepairingBuilding(null);
            }
            attackQueued = null; //so we wont attack anyone when moving
            if (flowPath != null)
            {
                flowPath.PathDone(this);
            }
            GetComponent<Animator>().SetBool("Running", true);
            if (boid)
            {
                boid.SetIsOn(true);
            }
            flowPath = tile;
        }

        public void SetFlowPathAttack(OnlineFlowFieldGraphManager tile)
        {
            if (flowPath != null)
            {
                flowPath.PathDone(this);
            }
            GetComponent<Animator>().SetBool("Running", true);
            if (boid)
            {
                boid.SetIsOn(true);
            }
            flowPath = tile;
        }

        public void MoveMessage(Vector2 pos)
        {
            GameEvents.current.GetMessage($"m/{GetListId()}/{pos}/{GetId()}");
        }

        public void SetSelected(bool val)
        {
            if (val)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 100, 255, 255);

            }
            else
            {
                ChangeShadow();
            }
        }

        public void TargetEnter(OnlineUnit enemy)
        {
            if (attackQueued == null)
            {
                attackQueued = enemy;
                //GameEvents.current.GetMessage($"a/u/{GetListId()}/{enemy.GetListId()}/{enemy.GetId()}/{GetId()}"); //attack message
            }
            else
            {
                enemiesInRange.Add(enemy);
            }
        }

        public void TargetExit(OnlineUnit enemy)
        {
            if (enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }

        private void OnTriggerEnter2D(Collider2D target)
        {
            if ((target.GetComponent<OnlineUnitRadius>() != null && (target.GetComponent<OnlineUnitRadius>().GetRadiusId() != GetId())) ||
                (target.GetComponent<OnlineTurretRadius>() != null && (target.GetComponent<OnlineTurretRadius>().GetRadiusId() != GetId())))
            {
                IncreaseUnitsLooking();
            }
            if (target.GetComponent<OnlineAura>() && target.GetComponent<OnlineAura>().GetId() == GetId())
            {
                hero = target.gameObject.GetComponentInParent<OnlineHero>();
                StartCoroutine(Buff());
            }
        }

        private void OnTriggerExit2D(Collider2D target)
        {
            if ((target.GetComponent<OnlineUnitRadius>() != null && (target.GetComponent<OnlineUnitRadius>().GetRadiusId() != GetId())) ||
                (target.GetComponent<OnlineTurretRadius>() != null && (target.GetComponent<OnlineTurretRadius>().GetRadiusId() != GetId())))
            {
                DecreaseUnitsLooking();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (flying)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>());
            }
            else if (!collision.gameObject.GetComponent<OnlineUnit>())
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>()); //Used to fix unit stucking to buildings and obstacles
            }
            else if (collision.gameObject.GetComponent<OnlineUnit>() && collision.gameObject.GetComponent<OnlineUnit>().GetId() == GetId())
            {
                /*if (isMoving)
                {
                    OnlineUnit unit = collision.gameObject.GetComponent<OnlineUnit>();
                    if (Vector2.Distance(unit.transform.position, GetComponent<UnitAI>().GetPath()) < 0.8f && unit.isMoving == false)
                    {
                        Vector2 direction = (collision.transform.position - transform.position).normalized;
                        Vector2 force = direction * Time.deltaTime;
                        transform.position -= (Vector3)force * 10;
                        GetComponent<UnitAI>().SetPath(transform.position);
                        MovementStop();
                    }
                }
                else
                {
                    //Vector2 direction = (collision.transform.position - transform.position).normalized;
                    //Vector2 force = direction * Time.deltaTime;
                    float x = transform.position.x - collision.transform.position.x;
                    float y = transform.position.y - collision.transform.position.y;
                    //Move(transform.position + new Vector3(x, y, -1));
                    MoveWhileAttack(transform.position + new Vector3(x, y, -1)); //if unit is attacking, it should continue attacking
                }*/
                if (flowPath && flowPath.FindUnit(collision.transform.GetComponent<OnlineUnit>()))
                {
                    PathDone();
                }
                if (!isMoving && Vector2.Distance(transform.position, ai.target.position) < 0.5f)
                {
                    float x = transform.position.x - collision.transform.position.x;
                    float y = transform.position.y - collision.transform.position.y;
                    Move(transform.position + new Vector3(x, y, -1));
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (gameObject != null)
            {
                if (collision != null && collision.gameObject.GetComponent<OnlineUnit>())
                {
                    OnlineUnit unit = collision.gameObject.GetComponent<OnlineUnit>();
                    if (Vector2.Distance(unit.transform.position, GetComponent<UnitAI>().GetPath()) < 0.8f && unit.isMoving == false)
                    {
                        Vector2 direction = (collision.transform.position - transform.position).normalized;
                        Vector2 force = direction * Time.deltaTime;
                        transform.position -= new Vector3(force.x, force.y, 0) * 5;
                        GetComponent<UnitAI>().SetPath(transform.position);
                        MovementStop();
                    }
                }
                if (collision.gameObject.CompareTag("PlayerBuilding"))
                {
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>()); //Used to fix unit stucking to buildings and obstacles
                }
            }
        }

        private IEnumerator Buff()
        {
            int oldDamage = damage;
            damage += (damage / 2); //increase damage by 50 percent
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 135, 0, 255);
            yield return new WaitForSeconds(10f);
            hero = null;
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            damage = oldDamage;
        }

        public void SlowDebuff()
        {
            if (!inflicted)
            {
                inflicted = true;
                ai.speed /= 2;
                StartCoroutine(Slow());
            }
        }

        private IEnumerator Slow()
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(50, 255, 50, 255);
            yield return new WaitForSeconds(3f);
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            inflicted = false;
            ai.speed *= 2;
        }

        public Animator GetAnimator()
        {
            return animator;
        }

        public void MovementStop()
        {
            animator.SetBool("Running", false);
            SetIsMoving(false);
        }

        public void ResetIsAttacking() => cooldown = false;

        public bool GetCooldown() => cooldown;

        public void SetCooldown(bool value) => cooldown = value;

        [Rpc(SendTo.ClientsAndHost)]
        public void FreezeRpc()
        {
            attackQueued = null;
            GetComponent<UnitAI>().moving = false;
            GetComponent<UnitAI>().target.position = transform.position;

            cantMove = true; //used so they cant attack
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            dest.transform.position = transform.position;
            StartCoroutine(Stun());
        }

        private IEnumerator Stun()
        {
            if (GetUnitsLooking() > 0 || IsOwner)
                GetComponent<SpriteRenderer>().color = new Color32(0, 200, 255, 255);
            yield return new WaitForSeconds(5f);
            cantMove = false;
            if (GetUnitsLooking() > 0 || IsOwner)
                GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        }

        public void WeakenEnter()
        {
            GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 255);
            GetComponent<OnlineUnit>().damage /= 2;
            GetComponent<UnitAI>().speed /= 2;
        }

        public void WeakenExit()
        {
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            GetComponent<OnlineUnit>().damage *= 2;
            GetComponent<UnitAI>().speed *= 2;
        }

        public OnlineTarget GetAttackQueued() => attackQueued;

        public void SetAttackQueued(OnlineTarget target)
        {
            attackQueued = target;
        }

        public void SetIsMoving(bool val) => isMoving = val;

        public void Upgrade(int id, float val)
        {
            switch (id)
            {
                case 0:
                    damage += (int)val;
                    break;
                case 1:
                    defense += (int)val;
                    break;
                case 2:
                    splashObject.GetComponent<CircleCollider2D>().radius += val;
                    break;
                case 3:
                    if (val == 1)
                    {
                        armorPiercing = true;
                    }
                    break;
            }
        }

        public void ResetFlowField(Vector2 pos)
        {
            if (flowPath)
            {
                flowPath.UpdateGraph();
                StartCoroutine(FlowWait());
            }
            else
            {
                SetFlowPath(OnlineFlowFieldGraph.Instance.SetNodePoint(pos, new List<OnlineUnit> { this }));
                flowPath.SetUnitsCount(1);
                flowPath.SetLeader(this);
                StartCoroutine(FlowWait());
            }
        }

        private IEnumerator FlowWait()
        {
            yield return new WaitForSeconds(0.6f);
            flowCooldown = false;
        }

        public OnlineFlowFieldGraphManager GetFlowPath()
        {
            return flowPath;
        }

        public void SetBoidOn()
        {
            if (boid)
            {
                boid.SetIsOn(true);
            }
            else
            {
                transform.GetChild(transform.childCount - 1).GetComponent<OnlineBoid>().SetIsOn(true);
            }
        }

        public void PathDone()
        {
            flowPath.PathDone(this);
            animator.SetBool("Running", false);
            flowPath = null;
            boid.SetIsOn(false);
        }

        public Vector3 ApplySpeed(Vector2 direction)
        {
            return direction * ai.speed * Time.deltaTime / 200;
        }

        public void VisibilityControl() //use the radius of others instead of yours
        {
            Collider2D[] results = new Collider2D[10];
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.NoFilter();
            contactFilter.maxDepth = 0;
            contactFilter.minDepth = -1;
            GetComponent<CircleCollider2D>().OverlapCollider(contactFilter, results);
            bool isVisible = false;
            foreach (Collider2D result in results)
            {
                if (result)
                {
                    if ((result.GetComponent<OnlineUnitRadius>() && result.GetComponent<OnlineUnitRadius>().GetUnit().GetId() != GetId()) || result.GetComponent<OnlineTurretRadius>() && result.GetComponent<OnlineTurretRadius>().GetBuilding().GetId() != GetId())
                    {
                        isVisible = true;
                        break;
                    }
                }
            }

            if(!isVisible)
            {
                GetComponent<OnlineTarget>().FixVanish();
            }
        }
    }
}