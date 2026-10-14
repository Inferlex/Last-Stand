using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeath : MonoBehaviour
{
    public GameObject[] effects;

    void Start()
    {
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        Instantiate(effects[0], transform.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        Instantiate(effects[1], transform.position, Quaternion.identity);
        GameEvents.current.GameFinished(true);
        Destroy(gameObject);
    }
}
