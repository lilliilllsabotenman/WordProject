using System;
using System.Threading.Tasks;
using MyTween;
using UnityEngine;

namespace NovelGameDialogue
{
    public sealed class NovelScheduler
    {
        private readonly IAnimationTargetResolver targetResolver;
        private readonly IAnimationConverter animationConverter;
        private readonly IRuntimeCommandConverter runtimeCommandConverter;
        private readonly IAnimationExecuter animationExecuter;

        public NovelScheduler(
            IAnimationTargetResolver targetResolver,
            IAnimationConverter animationConverter,
            IRuntimeCommandConverter runtimeCommandConverter,
            IAnimationExecuter animationExecuter)
        {
            this.targetResolver = targetResolver ?? throw new ArgumentNullException(nameof(targetResolver));
            this.animationConverter = animationConverter ?? throw new ArgumentNullException(nameof(animationConverter));
            this.runtimeCommandConverter = runtimeCommandConverter ?? throw new ArgumentNullException(nameof(runtimeCommandConverter));
            this.animationExecuter = animationExecuter ?? throw new ArgumentNullException(nameof(animationExecuter));
        }

        public async Task ExecuteAnimationAsync(AnimationActionData action)
        {
            if (action == null || action.Character == null || action.Animation == null)
            {
                return;
            }

            AnimationTargetCapabilities target = targetResolver.Resolve(action.Character);
            if (target == null)
            {
                Debug.LogWarning($"Character renderer not found for key: {action.Character.name}");
                return;
            }

            AnimationRuntimeData runtimeData = animationConverter.Convert(action.Animation, target);
            IRuntimeCommand command = runtimeCommandConverter.Convert(runtimeData);
            await animationExecuter.Execute(command);
        }
    }
}
