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
    }
}
