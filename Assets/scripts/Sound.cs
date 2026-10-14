using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string soundName;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(0f, 0.3f)]
    public float pitch;

    [HideInInspector]
    public AudioSource source;
}
