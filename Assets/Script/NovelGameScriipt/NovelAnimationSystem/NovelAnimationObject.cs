using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NovelAnimationObject : MonoBehaviour
{
    [Header("アニメーションキー")]
    public AnimationKey key;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public bool TryGetAnimator(out Animator result)
    {
        if (animator == null) animator = GetComponent<Animator>();
        result = animator;
        return result != null;
    }
}
