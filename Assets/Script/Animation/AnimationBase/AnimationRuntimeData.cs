using UnityEngine;

namespace MyTween
{
    public abstract class AnimationRuntimeData
    {
        public float Duration;
        public EasingConfigOption Easing;
    }

    public sealed class MoveRuntimeData : AnimationRuntimeData
    {
        public IPositionTarget Target;
        public Vector3 Start;
        public Vector3 End;
    }

    public sealed class FadeRuntimeData : AnimationRuntimeData
    {
        public IAlphaTarget Target;
        public float Start;
        public float End;
    }

    public sealed class ScaleRuntimeData : AnimationRuntimeData
    {
        public IScaleTarget Target;
        public Vector3 StartScale;
        public Vector3 EndScale;
    }
}
