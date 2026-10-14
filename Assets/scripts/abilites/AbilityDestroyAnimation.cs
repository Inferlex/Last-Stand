using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDestroyAnimation : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Die());
    }
    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }
}
