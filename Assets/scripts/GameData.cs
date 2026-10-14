using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int[] highScores;
    public bool[] missionsUnlocked;
    public float soundVolume;
    public float musicVolume;

    public GameData (bool[] missionsUnlocked)
    {
        this.missionsUnlocked = missionsUnlocked;
    }

    public void SetMissionUnlocked(int index, bool value)
    {
        missionsUnlocked[index] = value;
        ProgressionSystem.SaveProgression(this);
    }

    public void CheckHighscore(int index, int score)
    {
        if(highScores[index] < score)
        {
            highScores[index] = score;
        }
        ProgressionSystem.SaveProgression(this);
    }
}
