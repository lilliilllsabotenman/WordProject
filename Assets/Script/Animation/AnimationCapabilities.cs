using UnityEngine;
using UnityEngine.UI;

namespace MyTween
{
    public interface IAlphaTarget
    {
        float Alpha { get; set; }
    }

    public interface IPositionTarget
    {
        Vector3 Position { get; set; }
    }

    public interface IScaleTarget
    {
        Vector3 Scale { get; set; }
    }

    public interface ISpriteTarget
    {
        Sprite Sprite { get; set; }
    }

    public sealed class SpriteRendererAdapter : IAlphaTarget, ISpriteTarget, IPositionTarget, IScaleTarget
    {
        private readonly SpriteRenderer spriteRenderer;

        public SpriteRendererAdapter(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
        }

        public float Alpha
        {
            get => spriteRenderer.color.a;
            set
            {
                var c = spriteRenderer.color;
                c.a = Mathf.Clamp01(value);
                spriteRenderer.color = c;
            }
        }

        public Sprite Sprite
        {
            get => spriteRenderer.sprite;
            set => spriteRenderer.sprite = value;
        }

        public Vector3 Position
        {
            get => spriteRenderer.transform.position;
            set => spriteRenderer.transform.position = value;
        }

        public Vector3 Scale
        {
            get => spriteRenderer.transform.localScale;
            set => spriteRenderer.transform.localScale = value;
        }
    }

    public sealed class ImageAdapter : IAlphaTarget
    {
        private readonly Image image;

        public ImageAdapter(Image image)
        {
            this.image = image;
        }

        public float Alpha
        {
            get => image.color.a;
            set
            {
                var c = image.color;
                c.a = Mathf.Clamp01(value);
                image.color = c;
            }
        }
    }

    public sealed class AnimationTargetCapabilities
    {
        public IAlphaTarget AlphaTarget;
        public IPositionTarget PositionTarget;
        public IScaleTarget ScaleTarget;
        public ISpriteTarget SpriteTarget;
    }
}
