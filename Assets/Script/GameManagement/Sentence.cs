using System;
using System.Collections.Generic;
using UnityEngine;

public enum TextType
{
    Noun,
    Verb,
    Particle
}

[CreateAssetMenu(menuName = "Sentence/WordAsset")]
public class WordAsset : ScriptableObject
{
    [Header("Display text shown in UI")]
    [SerializeField] private string displayText;

    [Header("Grammar type")]
    [SerializeField] private TextType textType;

    public string DisplayText => displayText;
    public TextType Type => textType;

    public string getText()
    {
        return displayText;
    }
}

public enum SentenceElementType
{
    FixedText,
    Slot
}

[Serializable]
public class SentenceSlotDefinition
{
    public WordAsset answer;
    public TextType acceptType;
}

[Serializable]
public class SentenceElement
{
    public SentenceElementType type;
    public string fixedText;

    // Current format
    public WordAsset slotDefinition;

    // Legacy format compatibility (old assets used slotDefinition.answer)
    public SentenceSlotDefinition legacySlotDefinition;

    public WordAsset GetSlotWord()
    {
        if (slotDefinition != null)
        {
            return slotDefinition;
        }

        return legacySlotDefinition != null
            ? legacySlotDefinition.answer
            : null;
    }

    public bool IsValidInputSlot()
    {
        return type == SentenceElementType.Slot && GetSlotWord() != null;
    }
}
