using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardLayoutEntry
{
    public string displayText;
    public bool isSlot;
    public Vector2 position;  // スプライト中心からの相対座標
    public Vector2 size = new Vector2(100f, 50f);
    public PlayerTextSpriteData spriteData;
}

[CreateAssetMenu(menuName = "Word/SentenceLayoutData")]
public class SentenceLayoutData : ScriptableObject
{
    public Sprite sprite;
    public List<CardLayoutEntry> entries = new();
}