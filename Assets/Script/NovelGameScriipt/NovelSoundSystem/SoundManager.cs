using UnityEngine;
using System.Threading.Tasks;
using MyTween;

public class SoundManager
{
    private AudioSource current;
    private AudioSource next;
    private readonly AudioSource seSource;
    private readonly SoundExecuter executer = new();

    public SoundManager(AudioSource bgmSourceA, AudioSource bgmSourceB, AudioSource seSource)
    {
        current = bgmSourceA;
        next = bgmSourceB;
        this.seSource = seSource;
    }

    public async Task Play(SoundData sound, float bgmVolumeCap = 1f)
    {
        if (sound == null || sound.audioClip == null)
        {
            Debug.LogWarning("Play: SoundDataが未設定です。再生をスキップします。");
            return;
        }

        if (sound.delay > 0f)
        {
            await TweenActions.AnimateAsync(sound.delay, null, _ => { });
        }

        switch (sound.type)
        {
            case SoundType.BGM:
                await SwitchBGM(sound, bgmVolumeCap);
                break;
            case SoundType.SE:
                executer.PlayOneShot(seSource, sound.audioClip);
                break;
        }
    }

    private async Task SwitchBGM(SoundData sound, float volumeCap)
    {
        float targetVolume = Mathf.Min(sound.volume, volumeCap);

        if (current.clip == sound.audioClip)
        {
            current.volume = targetVolume;
            return;
        }

        await executer.CrossFadeAsync(current, next, sound.audioClip, sound.fadeDuration, targetVolume);

        (current, next) = (next, current);
    }
}
