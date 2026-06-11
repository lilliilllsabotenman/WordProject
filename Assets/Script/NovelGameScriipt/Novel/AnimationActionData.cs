using MyTween;
using UnityEngine;

namespace NovelGameDialogue
{
    [CreateAssetMenu(menuName = "Novel/Animation Action")]
    public sealed class AnimationActionData : NovelActionData
    {
        public CharacterKey Character;
        public AnimationClipData Animation;
    }
}
