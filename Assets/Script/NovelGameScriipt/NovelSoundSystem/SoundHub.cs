using UnityEngine;

public interface ISoundPlayer
{
    void Play(SoundData sound, float bgmVolumeCap = 1f);
}

public class SoundHub : MonoBehaviour, ISoundPlayer
{
    [Header("BGM再生用AudioSource(クロスフェード用に2つ)")]
    [SerializeField] private AudioSource bgmSourceA;
    [SerializeField] private AudioSource bgmSourceB;

    [Header("SE再生用AudioSource")]
    [SerializeField] private AudioSource seSource;

    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = new SoundManager(bgmSourceA, bgmSourceB, seSource);
    }

    public void Play(SoundData sound, float bgmVolumeCap = 1f)
    {
        if (sound == null || soundManager == null) return;

        _ = soundManager.Play(sound, bgmVolumeCap);
    }
}
