using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f,.25f)]
    public float volume;
    [HideInInspector]
    public AudioSource source;
    public bool loop;
}
