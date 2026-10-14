using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    void Awake()
    {
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            //s.source.pitch = s.pitch;
        }
    }

    public void PlaySound(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
        {
            Debug.Log("wh");
            s.source.Play();
        }
    }

    private Sound FindSound(string name)
    {
        foreach(Sound s in sounds)
        {
            if (s.soundName == name)
                return s;
        }
        return null;
    }
}
