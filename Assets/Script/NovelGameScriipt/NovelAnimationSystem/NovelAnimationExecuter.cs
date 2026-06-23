using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using NovelGameDialogue;

public struct SpriteRendererSnapshot
{
    public Sprite Sprite;
    public Color Color;

    public SpriteRendererSnapshot(SpriteRenderer renderer)
    {
        Sprite = renderer.sprite;
        Color = renderer.color;
    }

    public void Apply(SpriteRenderer renderer)
    {
        renderer.sprite = Sprite;
        renderer.color = Color;
    }
}
public struct TransformSnapshot
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public TransformSnapshot(Transform transform)
    {
        Position = transform.localPosition;
        Rotation = transform.localRotation;
        Scale = transform.localScale;
    }

    public void Apply(Transform transform)
    {
        transform.localPosition = Position;
        transform.localRotation = Rotation;
        transform.localScale = Scale;
    }
}

public class NovelAnimationExecuter
{
    private PlayableGraph graph;

    public async Task ExecuteAsync(Animator animator, AnimationClip clip)
    {
        animator.runtimeAnimatorController = null;

        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "NovelAnimation", animator);
        AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(playable);

        graph.Play();

        Dictionary<string, object> transformSnap = new();
        Dictionary<string, object> spriteRendererSnap = new();

        float elapsed = 0f;
        while (elapsed + 0.01f < clip.length)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
            graph.Evaluate(Time.deltaTime);
        }


        playable.SetTime(clip.length);

        // ★ Evaluate直後・Destroy直前にスナップショット
        TransformSnapshot _transform = new TransformSnapshot(animator.transform);
        SpriteRendererSnapshot _spriteRenderer = new SpriteRendererSnapshot(animator.GetComponent<SpriteRenderer>());


        playable.SetTime(clip.length);
        graph.Evaluate(Time.deltaTime); 
        graph.Destroy();

        _transform.Apply(animator.gameObject.transform);
        _spriteRenderer.Apply(animator.gameObject.GetComponent<SpriteRenderer>());


        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButton(0));
    }
}