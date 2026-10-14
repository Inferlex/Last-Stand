using Assets.Src;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject screen;
    public Slider loadingSlider;
    public Text loadingText;

    public void LoadLevel(int index)
    {
        StartCoroutine(LoadAsynchronously(index)); 
    }

    IEnumerator LoadAsynchronously (int index)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        screen.SetActive(true);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            loadingText.text = progress * 100f + "%";
            yield return null;
        }
    }
}
