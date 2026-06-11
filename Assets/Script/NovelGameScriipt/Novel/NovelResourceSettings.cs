using UnityEngine;
using NovelGameDialogue;

[CreateAssetMenu(menuName = "Novel/NovelGameOption")]
public class NovelResourceSettings : ScriptableObject
{
    [Header("ノベルゲーム共通設定")]
    public TextDialogueOption option;
}