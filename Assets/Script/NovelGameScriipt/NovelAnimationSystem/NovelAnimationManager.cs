using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class NovelAnimationManager
{
    private event Action AnimFinished;
    private Dictionary<AnimationKey, Animator> animatorData = new();
    private NovelAnimationExecuter animationExecuter = new();
    
    public NovelAnimationManager(NovelAnimationObject[] objects)
    {
        foreach(NovelAnimationObject anim in objects)
        {
            if(anim.TryGetAnimator(out Animator animator)) 
            {
                animatorData[anim.key] = animator;

            }
        }
    }

    public void Subscribe(Action action)
    {
        AnimFinished += action;
    }


    public async Task PlayAnimation(NovelAnimationSettings anim)
    {
        if (!animatorData.TryGetValue(anim.animationKey, out Animator animator))
        {
            Debug.LogError($"Animatorが見つかりません: {anim.animationKey}");
            return;
        }
        Debug.Log("ExecuteAnimation");
        await animationExecuter.ExecuteAsync(animatorData[anim.animationKey], anim.animationClip);
        AnimFinished?.Invoke();
    }
}