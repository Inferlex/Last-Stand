using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deatacher : MonoBehaviour //Deatach the object from parent on start
{
    public GameObject parent;

    void Start()
    {
        transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (!parent)
            Destroy(gameObject);
    }
}
