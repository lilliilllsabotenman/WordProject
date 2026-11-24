using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class CardLayoutEntry
{
    public string displayText;
    public bool isSlot;
    public Vector2 position;  // スプライト中心からの相対座標
    public Vector2 size = new Vector2(100f, 50f);
    public PlayerTextSpriteData spriteData;

    [Header("Editorプレビュー用(実行時の見た目には影響しません)")]
    public WordAsset previewWord;

    [Header("フォントサイズ手動上書き(自動縮小より優先)")]
    public bool overrideFontSize;
    public float fontSizeOverride = 24f;
}

[CreateAssetMenu(menuName = "Word/SentenceLayoutData")]
public class SentenceLayoutData : ScriptableObject
{
    public Sprite sprite;
    public Vector2 spriteSize = Vector2.zero; // (0,0)ならsprite本来のサイズを使う
    public List<CardLayoutEntry> entries = new();

    [Header("全体倍率")]
    public float layoutScale = 1f; // 座標・サイズ・文字サイズ全体にかかる倍率

    [Header("文字のデフォルト設定")]
    public float fontSize = 24f;
    public Color fontColor = Color.black;
    public TMP_FontAsset fontAsset;

    [Header("文字数によるフォント自動縮小")]
    public float minFontSize = 14f;
    public int charsAtMaxFontSize = 2;
    public int charsAtMinFontSize = 8;

    [Header("カード背景画像プレハブ(RectTransform+Imageを持つ想定。未設定なら背景なし)")]
    public GameObject cardBackgroundPrefab;
}