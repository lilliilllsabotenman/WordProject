using UnityEngine;

public enum SoundType
{
    BGM,
    SE
}

[CreateAssetMenu(menuName = "Novel/Sound")]
public class SoundData : NovelGameData
{
    public SoundType type;
    public AudioClip audioClip;
    public float fadeDuration = 1f;
    public float delay = 0f;

    [Range(0f, 1f)]
    public float volume = 1f;
}
