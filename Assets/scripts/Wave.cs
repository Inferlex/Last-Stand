using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    [SerializeField]
    private int[] enemyCount;
    [SerializeField]
    private int wavePos;

    public int GetPos() => wavePos;
    public int getEnemyCount(int index) => enemyCount[index];
}
