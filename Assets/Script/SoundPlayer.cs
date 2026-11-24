using UnityEngine;

[System.Serializable]
public class SoundPlayer
{
    [SerializeField]
    private AudioSource audioSource;

    public void SetPlayBGM(AudioClip Audio)
    {
        audioSource.clip = Audio;
        audioSource.Play();
    }
}