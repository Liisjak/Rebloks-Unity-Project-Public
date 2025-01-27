using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public Sound[] sounds;
    void Awake()
    {
        DontDestroyOnLoad(this);

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
        }

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void playSound(string name)
    {
        Sound sound = Array.Find(sounds, element => element.name == name);
        if (sound == null)
        {
            Debug.LogWarning("Sound: " +  name + " not found!");
            return;
        }
        sound.source.Play();
    }
}
