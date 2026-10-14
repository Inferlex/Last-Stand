using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineTentacleSwarm : OnlineAbility
{
    [SerializeField] private GameObject tentacle;
    private OnlineHero hero;

    // Update is called once per frame
    public override void Activate(OnlineHero hero)
    {
        this.hero = hero;
        StartCoroutine(SpawnTentacles());
    }

    private IEnumerator SpawnTentacles()
    {
        Vector2 position = transform.position;
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.4f);
            Vector2 newPosition = position + new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            SpawnTentacleRpc(newPosition);
        }
        Destroy(gameObject);
    }

    [Rpc(SendTo.Server)]
    private void SpawnTentacleRpc(Vector2 newPosition)
    {
        GameObject obj = Instantiate(tentacle, newPosition, Quaternion.identity);
        obj.GetComponent<OnlineTentacle>().Activate(hero);
        obj.GetComponent<NetworkObject>().Spawn();
    }
}
