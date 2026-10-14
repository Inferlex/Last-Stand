using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creator : Enemy
{
    [SerializeField]
    List<Enemy> unit;
    [SerializeField]
    private List<int> creationTime;

    private new void Start()
    {
        base.Start();
        StartCoroutine(SpawnUnit());
    }

    private IEnumerator SpawnUnit()
    {
        while (true)
        {
            int i = GenerateRandomInt(0, 3);
            yield return new WaitForSeconds(creationTime[i]);
            Instantiate(unit[i], new Vector3(transform.position.x, transform.position.y, -2f), Quaternion.identity);
        }     
    }

    private int GenerateRandomInt(int min, int max)
    {
        int rand = Random.Range(min, max);
        return rand;
    }
}
