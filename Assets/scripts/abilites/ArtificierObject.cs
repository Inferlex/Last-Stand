using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificierObject : MonoBehaviour
{
    public GameObject prefab;

    public GameObject Activate()
    {
        GameObject obj =  Instantiate(prefab, new Vector3(transform.position.x, transform.position.y, -1), Quaternion.identity);
        Destroy(gameObject);
        return obj;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
