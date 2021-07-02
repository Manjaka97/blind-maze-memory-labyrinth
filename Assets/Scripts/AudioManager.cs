using UnityEngine;
using System;

public class AudioManager: MonoBehaviour
{
    public Sound[] sounds;
    public Sound theme;
    public Sound ambient;
    public enum soundSetting
    {
        AllOn,
        FXOn,
        NoneOn
    }

    public soundSetting soundState;

    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        // Adding AudioSource to each sound
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent <AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }

        // Initializing sound setting
        if (!PlayerPrefs.HasKey("Sound"))
        {
            PlayerPrefs.SetInt("Sound", 0); //AllOn
            PlayerPrefs.Save();
        }

        soundState = (soundSetting)PlayerPrefs.GetInt("Sound");

        // Getting a reference to theme and ambient to be turned on or off separately
        theme = Array.Find(sounds, sound => sound.name == "theme");
        ambient = Array.Find(sounds, sound => sound.name == "ambient");
    }

    public void Play(string name)
    {
        if (soundState==soundSetting.AllOn || soundState == soundSetting.FXOn)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
                return;
            s.source.Play();
        } 
    }

    public void PlayTheme()
    {
        if (soundState==soundSetting.AllOn)
        {
            theme.source.Play();
            ambient.source.Play();
        }
        else
        {
            theme.source.Stop();
            ambient.source.Stop();
        }
    }
}
