using Assets.Src;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public Hero[] heroes;
    public GameObject[] faction;
    private Player player;
    public Text description;
    public Text missionStats;
    public GameObject[] missions;
    public bool[] missionUnlocked;
    public int[] highScores;
    private float soundVolume;
    private float musicVolume;
    public SettingsMenu sm;
    public OnlineHero[] onlineHeroes;
    public GameObject secMenu;
    private int maxNumberOfPlayers = 2;

    public void Start()
    {
        /*NetworkManager.Singleton.GetComponent<UnityTransport>().SetDebugSimulatorParameters(
            packetDelay: 220,
            packetJitter: 40,
            dropRate: 30);*/
        if (ProgressionSystem.LoadProgression() != null)
        {
            LoadProgression(); //is there is saved data, load it
        }
        else
        {
            SaveProgression();//else create new data
        }
    }

    public void SetHero(int i)
    {
        Player.hero = heroes[i];
    }

    public void SetFaction(int i)
    {
        Player.PlayerManager = faction[i];
    }

    public void PlayGame(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowDescription(string text)
    {
        description.text = text;
    }

    public void PlayTutorial()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowMissions()
    {
        int i = 0;
        foreach(GameObject mission in missions)
        {
            if (missionUnlocked[i])
            {
                mission.gameObject.SetActive(true);
            }
            else
                mission.gameObject.SetActive(false);
            i++;
        }
    }

    public void SaveProgression()
    {
        ProgressionSystem.SaveProgression(this);
    }

    public void LoadProgression()
    {
        try
        {
            GameData data = ProgressionSystem.LoadProgression();

            missionUnlocked = data.missionsUnlocked;
            highScores = data.highScores;
            soundVolume = data.soundVolume;
            musicVolume = data.musicVolume;
            sm.SetVolumes(soundVolume, musicVolume);
        }
        catch
        {
            Debug.Log("Corrupted save. Creating new save.");
            SaveProgression();
        }
    }

    public void SetMissionUnlocked(int index)
    {
        missionUnlocked[index] = true;
    }

    public void CheckHighscore(int index, int score)
    {
        if (highScores[index] < score)
        {
            highScores[index] = score;
        }
    }

    public void ShowHighscore(int index)
    {
        missionStats.text = "Highscore: " + highScores[index];
    }

    public void EraseDescription()
    {
        description.text = "";
        missionStats.text = "";
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
    }

    public float GetSoundVolume()
    {
        return soundVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public void SetMaxNumberOfPlayers(int i)
    {
        maxNumberOfPlayers = i + 2;       
    }

    public int GetMaxNumberOfPlayers()
    {
        return maxNumberOfPlayers;
    }
}
