using System;
using UnityEngine;

namespace MyTween
{
    public interface IAnimationConverter
    {
        AnimationRuntimeData Convert(AnimationClipData clip, AnimationTargetCapabilities target);
    }

    public sealed class AnimationConverter : IAnimationConverter
    {
        public AnimationRuntimeData Convert(AnimationClipData clip, AnimationTargetCapabilities target)
        {
            if (clip == null)
            {
                throw new ArgumentNullException(nameof(clip));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            switch (clip)
            {
                case MoveAnimationClip move:
                    if (target.PositionTarget == null)
                    {
                        throw new InvalidOperationException("Target does not support position.");
                    }

                    Vector3 startPosition = target.PositionTarget.Position;
                    Vector3 moveEnd = startPosition + ((Vector3)move.Direction.normalized * move.Distance);
                    return new MoveRuntimeData
                    {
                        Target = target.PositionTarget,
                        Start = startPosition,
                        End = moveEnd,
                        Duration = Mathf.Max(0f, move.Duration),
                        Easing = move.Easing
                    };

                case FadeAnimationClip fade:
                    if (target.AlphaTarget == null)
                    {
                        throw new InvalidOperationException("Target does not support alpha.");
                    }

                    return new FadeRuntimeData
                    {
                        Target = target.AlphaTarget,
                        Start = fade.FadeIn ? 0f : 1f,
                        End = fade.FadeIn ? 1f : 0f,
                        Duration = Mathf.Max(0f, fade.Duration),
                        Easing = fade.Easing
                    };

                case ScaleAnimationClip scale:
                    if (target.ScaleTarget == null)
                    {
                        throw new InvalidOperationException("Target does not support scale.");
                    }

                    return new ScaleRuntimeData
                    {
                        Target = target.ScaleTarget,
                        StartScale = scale.StartScale,
                        EndScale = scale.EndScale,
                        Duration = Mathf.Max(0f, scale.Duration),
                        Easing = scale.Easing
                    };

                default:
                    throw new NotSupportedException($"Unsupported clip type: {clip.GetType().Name}");
            }
        }
    }

    public interface IRuntimeCommandConverter
    {
        IRuntimeCommand Convert(AnimationRuntimeData runtimeData);
    }

    public sealed class RuntimeCommandConverter : IRuntimeCommandConverter
    {
        public IRuntimeCommand Convert(AnimationRuntimeData runtimeData)
        {
            if (runtimeData == null)
            {
                throw new ArgumentNullException(nameof(runtimeData));
            }

            switch (runtimeData)
            {
                case MoveRuntimeData move:
                    return new MoveRuntimeCommand(move);
                case FadeRuntimeData fade:
                    return new FadeRuntimeCommand(fade);
                case ScaleRuntimeData scale:
                    return new ScaleRuntimeCommand(scale);
                default:
                    throw new NotSupportedException($"Unsupported runtime data: {runtimeData.GetType().Name}");
            }
        }
    }
}
