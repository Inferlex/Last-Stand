using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Src;
using Unity.Netcode;

public class PauseMenu : MonoBehaviour
{
    public PlayerManager pm;
    public OnlinePlayerManager opm;

    public static bool gameIsPaused = false;

    public GameObject pauseMenuUI;

    public int nextMissionIndex;
    public Button finishButton;
    [SerializeField] private bool online;

    private void Start()
    {
        gameIsPaused = false;
        GameEvents.current.OnGameFinished += OnGameFinished;
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        if (!online)
        {
            Time.timeScale = 1f;
        }
        gameIsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        if (!online)
        {
            Time.timeScale = 0f;
        }
        gameIsPaused = true;
    }

    public void LoadMenu() => SceneManager.LoadScene(0);

    public void LoadMenuOnline()
    {
        opm.DisconnectFromGameRpc(opm.GetId());
        
        SceneManager.LoadScene(0);
    }

    public void QuitGame() => Application.Quit();

    public void QuitGameOnline()
    {
        opm.DisconnectFromGameRpc(opm.GetId());
        Application.Quit();
    }

    public bool IsPaused() => gameIsPaused;

    public void Save()
    {
        GameData data = ProgressionSystem.LoadProgression();
        if(nextMissionIndex != 3)
            data.SetMissionUnlocked(nextMissionIndex, true); //unlock next mission
        data.CheckHighscore(nextMissionIndex - 1, pm.GetScore());
        ProgressionSystem.SaveProgression(data);
    }

    public void OnGameFinished(bool value)
    {
        if (value)
        {
            finishButton.gameObject.SetActive(true);
        }
    }
}
