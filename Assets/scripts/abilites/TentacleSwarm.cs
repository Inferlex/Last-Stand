using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleSwarm : MonoBehaviour
{
    [SerializeField] private GameObject tentacle;

    // Update is called once per frame
    public void Activate()
    {
        StartCoroutine(SpawnTentacles());
    }

    private IEnumerator SpawnTentacles()
    {
        Vector2 position = transform.position;
        for (int i = 0; i < 16; i++)
        {
            yield return new WaitForSeconds(0.4f);
            Vector2 newPosition = position + new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            Instantiate(tentacle, newPosition, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
