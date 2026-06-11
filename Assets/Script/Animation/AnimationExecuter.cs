using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MyTween
{
    public interface IRuntimeCommand
    {
        Task ExecuteAsync();
    }

    public interface IAnimationExecuter
    {
        Task Execute(IRuntimeCommand command);
    }

    public sealed class AnimationExecuter : IAnimationExecuter
    {
        public Task Execute(IRuntimeCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return command.ExecuteAsync();
        }
    }

    public sealed class MoveRuntimeCommand : IRuntimeCommand
    {
        private readonly MoveRuntimeData data;

        public MoveRuntimeCommand(MoveRuntimeData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public async Task ExecuteAsync()
        {
            if (data.Target == null)
            {
                return;
            }

            await TweenActions.AnimateAsync(
                data.Duration,
                data.Easing,
                t => data.Target.Position = Vector3.LerpUnclamped(data.Start, data.End, t));
            data.Target.Position = data.End;
        }
    }

    public sealed class FadeRuntimeCommand : IRuntimeCommand
    {
        private readonly FadeRuntimeData data;

        public FadeRuntimeCommand(FadeRuntimeData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public async Task ExecuteAsync()
        {
            if (data.Target == null)
            {
                return;
            }

            await TweenActions.AnimateAsync(
                data.Duration,
                data.Easing,
                t => data.Target.Alpha = Mathf.Lerp(data.Start, data.End, t));
            data.Target.Alpha = data.End;
        }
    }

    public sealed class ScaleRuntimeCommand : IRuntimeCommand
    {
        private readonly ScaleRuntimeData data;

        public ScaleRuntimeCommand(ScaleRuntimeData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public async Task ExecuteAsync()
        {
            if (data.Target == null)
            {
                return;
            }

            await TweenActions.AnimateAsync(
                data.Duration,
                data.Easing,
                t => data.Target.Scale = Vector3.LerpUnclamped(data.StartScale, data.EndScale, t));
            data.Target.Scale = data.EndScale;
        }
    }

    public static class TweenActions
    {
        public static async Task AnimateAsync(float duration, EasingConfigOption easing, Action<float> onStep)
        {
            if (onStep == null)
            {
                return;
            }

            if (duration <= 0f)
            {
                onStep(1f);
                return;
            }

            var evaluator = new EasingEvaluator();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                await Task.Yield();

                elapsed += (easing != null && easing.UseUnscaledTime)
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                onStep(evaluator.Evaluate(t, easing));
            }
        }
    }
}
