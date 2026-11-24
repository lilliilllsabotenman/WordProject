using UnityEngine;
using System.Threading.Tasks;

public class SoundExecuter
{
    public async Task CrossFadeAsync(AudioSource from, AudioSource to, AudioClip clip, float duration, float targetVolume)
    {
        float fromStartVolume = from.volume;

        to.clip = clip;
        to.volume = 0f;
        to.Play();

        duration = Mathf.Max(duration, 0.01f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            from.volume = Mathf.Lerp(fromStartVolume, 0f, t);
            to.volume = Mathf.Lerp(0f, targetVolume, t);
        }

        from.Stop();
        from.volume = fromStartVolume;
        to.volume = targetVolume;
    }

    public void PlayOneShot(AudioSource source, AudioClip clip)
    {
        source.PlayOneShot(clip);
    }
}
