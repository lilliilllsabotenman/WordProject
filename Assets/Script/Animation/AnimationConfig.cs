using UnityEngine;

namespace MyTween
{
    public abstract class AnimationConfig : ScriptableObject
    {
    }

    public abstract class AnimationClipData : AnimationConfig
    {
    }

    [CreateAssetMenu(menuName = "MyAnimaiton/Move")]
    public sealed class MoveAnimationClip : AnimationClipData
    {
        public Vector2 Direction;
        public float Distance;
        public float Duration = 1f;
        public EasingConfigOption Easing;
    }

    [CreateAssetMenu(menuName = "MyAnimaiton/Fade")]
    public sealed class FadeAnimationClip : AnimationClipData
    {
        public bool FadeIn = true;
        public float Duration = 1f;
        public EasingConfigOption Easing;
    }

    [CreateAssetMenu(menuName = "MyAnimaiton/Scale")]
    public sealed class ScaleAnimationClip : AnimationClipData
    {
        public Vector3 StartScale = Vector3.one;
        public Vector3 EndScale = Vector3.one;
        public float Duration = 1f;
        public EasingConfigOption Easing;
    }
}
