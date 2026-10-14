using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Src.Units
{
    public class Unit : Target
    {
        public float fireRate;
        public int damage;
        public float attackRange;
        UnitAI ai;
        public Transform dest;
        private bool cooldown = false;
        private CircleCollider2D radius;
        private bool isMoving;
        private List<Enemy> enemiesInRange = new List<Enemy>();
        public int capacityCost;
        public bool armorPiercing;
        public bool splash;
        public GameObject splashObject;
        public bool flying;
        private bool inflicted = false;
        private Animator animator;
        private Hero hero;
        public int xp;

        Target attackQueued = null;

        private FlowFieldGraphManager flowPath;
        private bool flowCooldown;

        private Boid boid;

        public new void Start()
        {
            base.Start();

            ai = GetComponent<UnitAI>();
            dest.SetParent(null);
            radius = gameObject.transform.GetChild(0).GetComponent<CircleCollider2D>();
            if (!GetComponent<BossPart>() && !GetComponent<Boss>())
            {
                gameObject.transform.GetChild(0).GetComponent<UnitRadius>().SetUnit(this);
                ai.target = dest;
                //Move(dest.position); //only used for normal movement, not flowfield
            }
            gameObject.transform.GetChild(0).SetParent(null);

            animator = GetComponent<Animator>();
            if (CompareTag("PlayerUnit"))
            {
                GameEvents.current.UnitCreation(this);
                GameEvents.current.PointsEarn(points);
                //Upgrade();
                if (transform.GetChild(transform.childCount - 1).GetComponent<Boid>() != null)
                {
                    boid = transform.GetChild(transform.childCount - 1).GetComponent<Boid>();
                }
            }
        }

        public void Update()
        {
            if (attackQueued && !isMoving)
            {
                Attack(attackQueued);
            }
            if (!attackQueued && enemiesInRange.Count > 0) //no enemy locked but there are enemies close
            {
                attackQueued = enemiesInRange[0];
                enemiesInRange.Remove(enemiesInRange[0]);
            }
            if (flowPath && !cooldown)
            {
                FlowMove(transform.position);
            }
        }

        public void Attack(Target target) //attack doesnt work on building with player units
        {
            attackQueued = target;
            if (target.GetComponent<Unit>() && target.GetComponent<Unit>().flying && attackRange < 2) //cannot attack flying enemies in close combat
            {
                GameEvents.current.ShowMessage("Cannot attack flying creature with melee unit");
                return;
            }
            if (!target.GetComponent<Collider2D>().isActiveAndEnabled) //if the boss is burrowed or when the unit is creator
            {
                return;
            }
            if (!GetComponent<Enemy>() && target && attackQueued && !enemiesInRange.Contains(attackQueued.GetComponent<Enemy>()) && !target.GetComponent<Boss>()) //add old selected enemy to the list of enemies if in range
            {
                enemiesInRange.Add(attackQueued.GetComponent<Enemy>());
            }
            if (!cooldown) //if unit already attacked and is on cooldown
            {
                attackQueued = null;
                var distance = Vector2.Distance(target.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.position);
                if (distance > attackRange)
                {
                    if (GetComponent<Enemy>())
                    {
                        Move(target.GetComponent<Collider2D>().ClosestPoint(transform.position));
                    }
                    else if (!flowCooldown && flowPath && target.GetComponent<Building>() == null)
                    {
                        flowCooldown = true;
                        ResetFlowField(target.GetComponent<Collider2D>().ClosestPoint(transform.position));
                    }
                    else if(flowPath == null)
                    {
                        GenerateFlowField(target.GetComponent<Collider2D>().ClosestPoint(transform.position));
                    }

                    if (!GetComponent<Enemy>())
                    {
                        attackQueued = target;
                    }
                }
                else
                {
                    if (GetComponent<Creator>()) //creator cant attack
                        return;

                    cooldown = true;
                    GetComponent<AudioSource>().Play();

                    if (GetComponent<Enemy>())
                    {
                        Move(transform.position);
                        MovementStop();
                    }
                    else if (flowPath)
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
                    animator.CrossFade("Attack", 0, 0);
                    if (splash)
                    {
                        GameObject splash = Instantiate(splashObject, target.transform.position, Quaternion.identity);
                        splash.GetComponent<SplashDamage>().Activate(damage, GetId());
                    }
                    else
                    {
                        if (target.GetComponent<Enemy>() || target.GetComponent<Boss>())
                        {
                            if (gameObject.GetComponent<Acolyte>())
                            {
                                gameObject.GetComponent<Acolyte>().ShowAttack(target);
                            }
                            if (gameObject.GetComponent<Artificier>())
                            {
                                gameObject.GetComponent<Artificier>().ShowAttack(target);
                            }
                            if (target.GetComponent<Boss>())
                            {
                                target.TakeDamage(damage, armorPiercing);
                            }
                            else
                            {
                                if (hero == null)
                                    target.GetComponent<Enemy>().TakeDamage(damage, this, armorPiercing);
                                else
                                {
                                    target.GetComponent<Enemy>().TakeDamage(damage, hero, armorPiercing); //used for when hero kills an enemy to get xp
                                }
                            }
                        }
                        else
                        {
                            target.TakeDamage(damage, armorPiercing);
                            if (targetName == "Hunter" && target.GetComponent<Unit>())
                            {
                                target.GetComponent<Unit>().SlowDebuff();
                            }
                        }
                    }
                    StartCoroutine(Wait(target));
                }
            }
        }

        public IEnumerator Wait(Target target)
        {
            if (target.GetHp() > 0)
            {
                attackQueued = target;
            }
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
                attackQueued = null;
                //isMoving = true;
                ai.moving = true;
                ai.target.position = pos;
            }
        }

        public void FlowMove(Vector2 unitPos)
        {
            Node node = flowPath.GetNodeAtPosition(unitPos);
            Unit leader = flowPath.GetLeader(); //we might also want to get closer to leader when we can
            if(leader == this || Vector2.Distance(unitPos, leader.transform.position) > 2 || flowPath.FindUnit(leader))
            {
                //node = flowPath.GetNodeAtPosition(unitPos);
            }
            else if(node.GetCost() != ushort.MaxValue)
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
                    return;
                case 1:
                    direction = new Vector2(0, -1);
                    break;
                case 2:
                    direction = new Vector2(-1, -1);
                    break;
                case 3:
                    direction = new Vector2(-1, 0);
                    break;
                case 4:
                    direction = new Vector2(-1, 1);
                    break;
                case 5:
                    direction = new Vector2(0, 1);
                    break;
                case 6:
                    direction = new Vector2(1, 1);
                    break;
                case 7:
                    direction = new Vector2(1, 0);
                    break;
                case 8:
                    direction = new Vector2(1, -1);
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

        public Vector3 ApplySpeed(Vector2 direction)
        {
            return direction * ai.speed * Time.deltaTime/200;
        }

        public void SetSelected(bool val)
        {
            if (val)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 100, 255, 255);

            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
            }
        }

        public void TargetEnter(Enemy enemy)
        {
            if (attackQueued == null)
            {
                attackQueued = enemy;
            }
            else
            {
                enemiesInRange.Add(enemy);
            }
        }

        public void TargetExit(Enemy enemy)
        {
            if (enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }

        private void OnTriggerEnter2D(Collider2D target)
        {
            if (target.GetComponent<Aura>() && target.GetComponent<Aura>().GetId() == GetId())
            {
                hero = target.gameObject.GetComponentInParent<Hero>();
                StartCoroutine(Buff());
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (flying)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>());
            }
            /*else if (!collision.gameObject.GetComponent<Unit>())
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>()); //Used to fix unit stucking to buildings and obstacles
            }*/
            else if (collision.gameObject.GetComponent<Unit>() && !collision.gameObject.GetComponent<Enemy>())
            {
                if (flowPath && flowPath.FindUnit(collision.transform.GetComponent<Unit>()))
                {
                    PathDone();
                }
                if (isMoving) //for non flowpath movement
                {
                    Unit unit = collision.gameObject.GetComponent<Unit>();
                    if (Vector2.Distance(unit.transform.position, GetComponent<UnitAI>().GetPath()) < 0.8f && unit.isMoving == false)
                    {
                        //ai.SetPath(transform.position);
                        //MovementStop();
                        Move(transform.position);
                    }
                }
                else if (!isMoving && Vector2.Distance(transform.position, ai.target.position) < 0.5f)
                {
                    float x = transform.position.x - collision.transform.position.x;
                    float y = transform.position.y - collision.transform.position.y;
                    Move(transform.position + new Vector3(x, y, -1));
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision != null && collision.gameObject.GetComponent<Unit>())
            {
                Unit unit = collision.gameObject.GetComponent<Unit>();
                float x = transform.position.x - collision.transform.position.x;
                float y = transform.position.y - collision.transform.position.y;
                transform.position += new Vector3(x, y, 0) / 10;
                /*if (Vector2.Distance(unit.transform.position, GetComponent<UnitAI>().GetPath()) < 0.8f && unit.isMoving == false && unit.isMoving == true) //non flowfield
                {
                    Move(transform.position);
                }
                else
                {
                    AstarPath.active.AddWorkItem(new AstarWorkItem(() =>
                    {
                        var node = AstarPath.active.GetNearest(collision.transform.position).node;
                        if (node.Walkable != false)
                        { //used to fix setting nodes to walkable again
                            node.Walkable = false;
                            GameEvents.current.GridUpdate(node);
                        }
                    }));
                    if (!GetComponent<Boss>() && !GetComponent<BossPart>())
                    {
                        GetComponent<UnitAI>().StartNewPath();
                    }
                }*/
            }
            if (collision.gameObject.CompareTag("PlayerBuilding"))
            {
                //Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>()); //Used to fix unit stucking to buildings and obstacles
                float x = collision.gameObject.transform.position.x - transform.position.x;
                float y = collision.gameObject.transform.position.y - transform.position.y;
                transform.position += GetComponent<Unit>().ApplySpeed(new Vector2(-x, -y));
            }
        }

        private IEnumerator Buff()
        {
            int oldDamage = damage;
            damage += (damage / 2);
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 135, 0, 255);
            yield return new WaitForSeconds(10f);
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

        public void WeakenEnter()
        {
            GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 255);
            GetComponent<Unit>().damage /= 2;
            GetComponent<UnitAI>().speed /= 2;
        }

        public void WeakenExit()
        {
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            GetComponent<Unit>().damage *= 2;
            GetComponent<UnitAI>().speed *= 2;
        }

        public void WeakenTimed() => StartCoroutine(Debuff());

        private IEnumerator Debuff()
        {
            GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 255);
            GetComponent<Unit>().damage /= 2;
            GetComponent<UnitAI>().speed /= 2;

            yield return new WaitForSeconds(5f);

            GetComponent<Unit>().damage *= 2;
            GetComponent<UnitAI>().speed *= 2;
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        }

        public void MovementStop()
        {
            isMoving = false;
            animator.SetBool("Running", false);
        }

        public Animator GetAnimator()
        {
            return animator;
        }

        public void SetAttackQueued(Target target)
        {
            attackQueued = target;
        }

        public bool GetIsMoving() => isMoving;

        public void ResetIsAttacking() => cooldown = false;

        public bool GetCooldown() => cooldown;

        public void SetCooldown(bool value) => cooldown = value;

        public void SetIsMoving(bool value) => isMoving = value;

        public void SetFlowPath(FlowFieldGraphManager tile)
        {
            if (GetComponent<RepairUnit>())
            {
                GetComponent<RepairUnit>().SetRepairingBuilding(null);
            }
            attackQueued = null; //so we wont attack anyone when moving
            enemiesInRange.Clear();
            if (flowPath != null)
            {
                flowPath.PathDone(this);
            }
            GetComponent<Animator>().SetBool("Running", true);
            if (boid) //can get rid of if statement later
            {
                boid.SetIsOn(true);
            }
            flowPath = tile;
        }

        public void SetFlowPathAttack(FlowFieldGraphManager tile)
        {
            if (flowPath != null)
            {
                flowPath.PathDone(this);
            }
            GetComponent<Animator>().SetBool("Running", true);
            if (boid) //can get rid of if statement later
            {
                boid.SetIsOn(true);
            }
            flowPath = tile;
        }

        public void ResetFlowField(Vector2 pos)
        {
            flowPath.UpdateGraph();
            StartCoroutine(FlowWait());
        }

        public void GenerateFlowField(Vector2 pos)
        {
            SetFlowPath(FlowFieldGraph.Instance.SetNodePoint(pos, new List<Unit> { this }));
            flowPath.SetUnitsCount(1);
            flowPath.SetLeader(this);
            StartCoroutine(FlowWait());
        }

        private IEnumerator FlowWait()
        {
            yield return new WaitForSeconds(0.6f);
            flowCooldown = false;
        }

        public FlowFieldGraphManager GetFlowPath()
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
                transform.GetChild(transform.childCount - 1).GetComponent<Boid>().SetIsOn(true);
            }
        }

        public void PathDone()
        {
            flowPath.PathDone(this);
            animator.SetBool("Running", false);
            flowPath = null;
            boid.SetIsOn(false);
        }

        public Target GetAttackQueued() => attackQueued;
    }
}