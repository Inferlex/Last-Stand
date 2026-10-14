using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Units;

public class WaveManager : MonoBehaviour
{
    public Wave[] waves; //= { {4, 3, 1, 0, 0, 0, 0, 0}, {4, 4, 2, 1, 0, 0, 0, 0}, {0, 2, 4, 2, 1, 1, 0, 0} }; //the number of specific monsters for a specific wave (index = type of monster)
    public Boss boss;
    public int bossWave;
    public List<Unit> unitList = new List<Unit>(); //list of enemy types
    public Transform[] spawnPoints = new Transform[4]; //where the enemies will spawn (0: west, 1: south, 2: east, 3: north)
    private int currentEnemies = 0;
    private int wave = 0;
    private int numberOfWaves;
    private bool waveFinished = true;
    public AudioClip bossMusic;
    public AudioSource musicSource;

    private void Start()
    {
        numberOfWaves = waves.Length;
        GameEvents.current.OnDeath += OnDeath;
        GameEvents.current.OnEnemyCreation += OnCreation;
        GameEvents.current.NewWave(wave + "/" + numberOfWaves);
        StartCoroutine(WaitForWave());
    }

    private IEnumerator WaitForWave()
    {
        int time = 30;
        while(time > 0)
        {
            GameEvents.current.NewWave("Time left: " + time);
            yield return new WaitForSeconds(1f);           
            time--;
        }
        GameEvents.current.NewWave("Wave: " + (wave + 1) + "/" + numberOfWaves);
        waveFinished = false;
        MakeEnemies();
    }

    private void MakeEnemies()
    {
        if (wave != bossWave)
        {
            for (int i = 0; i < unitList.Count; i++) //for  every enemy type
            {
                for (int j = 0; j < waves[wave].getEnemyCount(i); j++) //number of enemies of a specific type
                {
                    Instantiate(unitList[i], spawnPoints[waves[wave].GetPos()]);
                }
            }
        }
        else
        {
            musicSource.clip = bossMusic;
            musicSource.Play();
            Instantiate(boss, spawnPoints[waves[wave].GetPos()]);
        }
    }

    private void OnDeath(Target target)
    {
        if (wave != bossWave)
        {
            if (target.GetComponent<Enemy>())
            {
                currentEnemies--;
            }
            if (currentEnemies == 0 && wave <= numberOfWaves && !waveFinished)
            {
                waveFinished = true;
                GameEvents.current.ShowMessage("Wave completed");
                wave++;
                if (wave < numberOfWaves)
                {
                    StartCoroutine(WaitForWave());
                }
                else
                {
                    GameEvents.current.GameFinished(true);
                }
            }
        }
    }

    private void OnCreation() => currentEnemies++;
}
