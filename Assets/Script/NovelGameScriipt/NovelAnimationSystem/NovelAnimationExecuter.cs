using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Threading.Tasks;
using NovelGameDialogue;

public class NovelAnimationExecuter
{
    private PlayableGraph graph;

    public async Task ExecuteAsync(
        Animator animator,
        AnimationClip clip)
    {
        animator.runtimeAnimatorController = null;

        graph = PlayableGraph.Create();

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "NovelAnimation", animator);

        AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);

        output.SetSourcePlayable(playable);

        graph.Play();

        await Task.Delay(Mathf.CeilToInt(clip.length * 1000));
        await Awaitable.EndOfFrameAsync();
        
        playable.SetTime(clip.length);
        graph.Evaluate();

        playable.SetSpeed(0);
        
        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButton(0));
    }
}