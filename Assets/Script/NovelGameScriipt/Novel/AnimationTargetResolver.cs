using System;
using System.Collections.Generic;
using MyTween;
using UnityEngine;
using UnityEngine.UI;

namespace NovelGameDialogue
{
    public interface IAnimationTargetResolver
    {
        AnimationTargetCapabilities Resolve(CharacterKey key);
    }

    public sealed class CharacterAnimationTargetResolver : IAnimationTargetResolver
    {
        private readonly Dictionary<CharacterKey, CharacterSpriteRenderer> characterRegistry;

        public CharacterAnimationTargetResolver(Dictionary<CharacterKey, CharacterSpriteRenderer> characterRegistry)
        {
            this.characterRegistry = characterRegistry ?? throw new ArgumentNullException(nameof(characterRegistry));
        }

        public AnimationTargetCapabilities Resolve(CharacterKey key)
        {
            if (key == null)
            {
                return null;
            }

            if (!characterRegistry.TryGetValue(key, out var characterRenderer) || characterRenderer == null)
            {
                return null;
            }

            var capabilities = new AnimationTargetCapabilities();
            SpriteRenderer spriteRenderer = characterRenderer.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                var spriteAdapter = new SpriteRendererAdapter(spriteRenderer);
                capabilities.AlphaTarget = spriteAdapter;
                capabilities.PositionTarget = spriteAdapter;
                capabilities.ScaleTarget = spriteAdapter;
                capabilities.SpriteTarget = spriteAdapter;
                return capabilities;
            }

            Image image = characterRenderer.GetComponent<Image>();
            if (image != null)
            {
                capabilities.AlphaTarget = new ImageAdapter(image);
            }

            Transform t = characterRenderer.transform;
            capabilities.PositionTarget = new TransformPositionAdapter(t);
            capabilities.ScaleTarget = new TransformScaleAdapter(t);
            return capabilities;
        }
    }

    public sealed class TransformPositionAdapter : IPositionTarget
    {
        private readonly Transform target;

        public TransformPositionAdapter(Transform target)
        {
            this.target = target;
        }

        public Vector3 Position
        {
            get => target.position;
            set => target.position = value;
        }
    }

    public sealed class TransformScaleAdapter : IScaleTarget
    {
        private readonly Transform target;

        public TransformScaleAdapter(Transform target)
        {
            this.target = target;
        }

        public Vector3 Scale
        {
            get => target.localScale;
            set => target.localScale = value;
        }
    }
}
