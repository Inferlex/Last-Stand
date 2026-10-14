using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    [SerializeField]
    private AudioSource clip;
    [SerializeField]
    private Slider[] sliders;

    public void SetSoundVolume(float volume) => audioMixer.SetFloat("SoundVolume", volume);

    public void SetMusicVolume(float volume) => audioMixer.SetFloat("MusicVolume", volume);

    public void PlayClip() => clip.Play();

    public void SetFullscreen(bool value) => Screen.fullScreen = value;

    public void SetVolumes(float soundVolume, float musicVolume)
    {
        SetSoundVolume(soundVolume);
        SetMusicVolume(musicVolume);
        sliders[0].value = soundVolume;
        sliders[1].value = musicVolume;
    }
}
