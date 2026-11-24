using System.Collections.Generic;
using System.Threading.Tasks;

[System.Serializable]
public class DataSequence
{
    public List<NovelAnimationSettings> animations = new();

    public async Task PlayAsync(NovelAnimationManager animationManager, bool autoAdvance = false)
    {
        if (animationManager == null) return;

        foreach (NovelAnimationSettings anim in animations)
        {
            await animationManager.PlayAnimation(anim, autoAdvance);
        }
    }
}
