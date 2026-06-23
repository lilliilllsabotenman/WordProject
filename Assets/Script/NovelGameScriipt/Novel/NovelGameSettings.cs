using UnityEngine;
using NovelGameDialogue;

[CreateAssetMenu(menuName = "Novel/DialogueSettings")]
public class NovelGameSettings : NovelGameData
{
    [Header("会話タイプ")]
    public DialogueType dialogueType;

    [Header("TextData")]
    public TextAsset DialogueCSV;
}