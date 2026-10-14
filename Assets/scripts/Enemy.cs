using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;
using Assets.Src;

public class Enemy : Unit
{
    private float distance = 1000;
    private Target lockedUnit;  
    private List<Target> unitsLooking = new();
    bool cantMove = false;

    public new void Start()
    {
        base.Start();
        GameEvents.current.EnemyCreation();
        gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        if(!GetComponent<BossPart>())
            StartCoroutine(ResetAggresion());
    }

    new void Update()
    {
        if (GetComponent<Rigidbody2D>().velocity == Vector2.zero)
        {
            GetAnimator().SetBool("Running", false);
        }
        else
        {
            GetAnimator().SetBool("Running", true);
        }
        if (!cantMove)
        {
            if (lockedUnit != null && (!lockedUnit.GetComponent<Hero>() || !lockedUnit.GetComponent<Hero>().GetIsDead()))
            {
                Attack(lockedUnit);
            }
            else
            {
                FindUnit();
            }
        }
    }

   IEnumerator ResetAggresion() //if enemy is not close to units, find closest unit to target
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if(unitsLooking.Count == 0)
            {
                FindUnit();
            }
        }
    }

    private void FindUnit()
    {

        List<Target> newList = new();
        foreach (Target target in unitsLooking) //sometimes destroyed target will remain in the list
        {
            if (target != null)
            {
                newList.Add(target);                
            }
        }
        unitsLooking = newList;
        Vanish();

        if (unitsLooking.Count> 0) //if an enemy has an unit in close proximity, target just them
        {
            FindLookingUnit();
            return;
        }
        Player pl = FindObjectOfType<Player>();

        if (pl.units.Count <= 0 && pl.buildings.Count <= 0)
        {
            return;
        }

        int i = 0;
        int selected = 0;
        if (pl.units.Count != 0)
        {          
            foreach (Unit unit in pl.units)
            {
                if (distance > Vector2.Distance(gameObject.transform.position, unit.transform.position))
                {
                    distance = Vector2.Distance(gameObject.transform.position, unit.transform.position);
                    selected = i;
                }
                i++;
            }
            lockedUnit = pl.units[selected];
        }

        int j = 0;
        foreach (Building building in pl.buildings)
        {
            if (building)
            {
                if (distance > Vector2.Distance(gameObject.transform.position, building.transform.position))
                {
                    distance = Vector2.Distance(gameObject.transform.position, building.transform.position);
                    selected = j;
                }
                j++;
            }
        }

        if (lockedUnit && lockedUnit.gameObject != null && distance < Vector2.Distance(gameObject.transform.position, lockedUnit.transform.position) && pl.buildings.Count != 0)
            lockedUnit = pl.buildings[selected];
    }

    private void FindLookingUnit()
    {
        int i = 0;
        int selected = 0;
        foreach (Target target in unitsLooking)
        {
            if (target != null)
            {
                if (distance > Vector2.Distance(gameObject.transform.position, target.transform.position))
                {
                    distance = Vector2.Distance(gameObject.transform.position, target.transform.position);
                    selected = i;
                }
                i++;
            }
        }

        lockedUnit = unitsLooking[selected];
    }

    new public void TakeDamage(int amount, Unit unit, bool penetration)
    {
        if (!penetration) //if the damage is not penetrative, defense will lessen the damage
        {
            int blocked = amount * defense / 100;
            amount -= blocked;
        }
        SetHp(GetHp() - amount);
        Instantiate(bloodEffect, transform.position, Quaternion.identity);
        StartCoroutine(HurtState());
        if (GetHp() <= 0)
        {
            if (unit.GetComponent<Hero>()) //if a hero kill the enemy, give him xp
            {
                unit.GetComponent<Hero>().GetXP(xp);
            }
            Die();
        }
        healthBar.ChangeSprites(maxHp, GetHp());

        if (!lockedUnit)
        {
            return;
        }
        if (!GetComponent<BossPart>() && unit.gameObject != null && Vector2.Distance(gameObject.transform.position, unit.transform.position) < Vector2.Distance(gameObject.transform.position, lockedUnit.transform.position))
        {
            lockedUnit = unit; //attacking unit gets targeted when its closer than the first targeted one
        }
    }


    public void Reveal() //makes enemy visible
    {
        if (GetComponent<BossPart>())
        {
            GetComponent<BossPart>().Reveal();
            return;
        }
        if (unitsLooking.Count > 0)
        {
            healthBar.ChangeTransparency(true);
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 100);
            gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(0, 112, 255, 255);
        }
    }

    public void Vanish() //makes enemy invisible
    {
        if (GetComponent<BossPart>())
        {
            GetComponent<BossPart>().Vanish();
            return;
        }
        if (unitsLooking.Count == 0)
        {
            healthBar.ChangeTransparency(false);
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
            gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if(target.GetComponent<UnitRadius>() && target.GetComponent<UnitRadius>().GetUnit() && target.GetComponent<UnitRadius>().GetUnit().GetId() != 99)
        {
            unitsLooking.Add(target.GetComponent<UnitRadius>().GetUnit());
            Reveal();
        }
        else if (target.GetComponent<TurretRadius>() && target.GetComponent<TurretRadius>().GetBuilding().CompareTag("PlayerBuilding"))
        {
            unitsLooking.Add(target.GetComponent<TurretRadius>().GetBuilding());
            Reveal();
        }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target.GetComponent<UnitRadius>() && target.GetComponent<UnitRadius>().GetUnit())
        {
            unitsLooking.Remove(target.GetComponent<UnitRadius>().GetUnit());
            Vanish();
        }
        else if (target.GetComponent<TurretRadius>() && target.GetComponent<TurretRadius>().GetBuilding())
        {
            unitsLooking.Remove(target.GetComponent<TurretRadius>().GetBuilding());
            Vanish();
        }
    }

    public new void Die()
    {
        GameEvents.current.PointsEarn(points);
        base.Die();
    }

    public void Freeze()
    {
        cantMove = true;
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        dest.transform.position = transform.position;
        StartCoroutine(Stun());
    }

    private IEnumerator Stun()
    {
        if(unitsLooking.Count > 0)
            GetComponent<SpriteRenderer>().color = new Color32(0, 200, 255, 255);
        yield return new WaitForSeconds(5f);
        cantMove = false;
        if (unitsLooking.Count > 0)
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }

    public void Weaken() => StartCoroutine(Debuff());

    private IEnumerator Debuff()
    {
        if (unitsLooking.Count > 0)
            GetComponent<SpriteRenderer>().color = new Color32(0, 0, 255, 255);
        GetComponent<Unit>().damage /= 2;
        GetComponent<UnitAI>().speed /= 2;

        yield return new WaitForSeconds(5f);

        GetComponent<Unit>().damage *= 2;
        GetComponent<UnitAI>().speed *= 2;

        if (unitsLooking.Count > 0)
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
    }

    public Target GetLockedUnit() => lockedUnit;

    public void SetLockedUnit(Target newTarget) => lockedUnit = newTarget;

    public int GetUnitsLookingCount() => unitsLooking.Count;
}
