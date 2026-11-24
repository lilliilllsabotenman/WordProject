using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System;
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

// SpriteRenderer / UI Image のどちらを持つ対象でも preserveFinalState を安全に扱うためのスナップショット。
public struct GraphicFinalStateSnapshot
{
    private readonly SpriteRenderer spriteRenderer;
    private readonly Image image;
    private readonly Sprite sprite;
    private readonly Color color;

    public GraphicFinalStateSnapshot(GameObject target)
    {
        spriteRenderer = target.GetComponent<SpriteRenderer>();
        image = spriteRenderer == null ? target.GetComponent<Image>() : null;

        if (spriteRenderer != null)
        {
            sprite = spriteRenderer.sprite;
            color = spriteRenderer.color;
        }
        else if (image != null)
        {
            sprite = image.sprite;
            color = image.color;
        }
        else
        {
            sprite = null;
            color = default;
        }
    }

    public void Apply()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
        }
        else if (image != null)
        {
            image.sprite = sprite;
            image.color = color;
        }
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

    public async Task ExecuteAsync(Animator animator, NovelAnimationSettings anim, Func<bool> autoAdvancePredicate = null)
    {
        AnimationClip clip = anim.animationClip;

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

        if (anim.preserveFinalState)
        {
            // ★ Evaluate直後・Destroy直前にスナップショット
            TransformSnapshot _transform = new TransformSnapshot(animator.transform);
            GraphicFinalStateSnapshot _graphic = new GraphicFinalStateSnapshot(animator.gameObject);

            playable.SetTime(clip.length);
            graph.Evaluate(Time.deltaTime);
            graph.Destroy();

            _transform.Apply(animator.gameObject.transform);
            _graphic.Apply();
        }
        else
        {
            playable.SetTime(clip.length);
            graph.Evaluate(Time.deltaTime);
            graph.Destroy();
        }


        if (autoAdvancePredicate == null || !autoAdvancePredicate())
        {
            await MassageExecuter.WaitUntilAsync(() =>
                Input.GetMouseButton(0)
                || (autoAdvancePredicate != null && autoAdvancePredicate()));
        }
    }
}