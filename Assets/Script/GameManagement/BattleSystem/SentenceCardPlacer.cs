using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 配置役：EnemyDataを見てカードを並べ、スロットカードを出現順のListで返す。
/// sentenceLayoutData があれば座標指定配置、無ければ従来通りcontainerへの並び順配置にフォールバックする。
/// </summary>
public class SentenceCardPlacer
{
    private readonly Transform container;
    private readonly Transform layoutAnchor;
    private readonly List<WordCardView> instances = new();
    private GameObject sharedBackground;

    public SentenceCardPlacer(Transform container, Transform layoutAnchor = null)
    {
        this.container = container;
        this.layoutAnchor = layoutAnchor;
    }

    public List<WordCardView> Place(SentenceTemplate template, SentenceLayoutData layoutData)
    {
        Clear();

        return layoutData != null
            ? PlaceByLayoutData(layoutData)
            : PlaceByTemplate(template);
    }

    private List<WordCardView> PlaceByLayoutData(SentenceLayoutData data)
    {
        var slotCards = new List<WordCardView>();
        var anchor = (RectTransform)(layoutAnchor != null ? layoutAnchor : container);
        var scale = data.layoutScale;

        CreateSharedBackground(data.sprite, data.spriteSize, anchor, scale);

        foreach (var entry in data.entries)
        {
            var card = WordCardView.Create(
                anchor,
                data.fontSize * scale,
                data.fontColor,
                data.fontAsset,
                data.minFontSize * scale,
                data.charsAtMaxFontSize,
                data.charsAtMinFontSize,
                data.cardBackgroundPrefab);
            var rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = entry.position * scale;
            rt.sizeDelta = entry.size * scale;

            if (entry.overrideFontSize)
                card.SetFontSizeOverride(entry.fontSizeOverride * scale);

            if (entry.isSlot)
            {
                card.SetEmpty(entry.spriteData);
                slotCards.Add(card);
            }
            else
            {
                card.SetFixedText(entry.displayText, entry.spriteData);
            }

            instances.Add(card);
        }

        return slotCards;
    }

    // 敵ごとの土台画像は1枚だけ生成し、各カードの下に敷く（カードごとに複製しない）。
    private void CreateSharedBackground(Sprite sprite, Vector2 spriteSize, RectTransform anchor, float scale)
    {
        if (sprite == null) return;

        var backgroundObject = new GameObject("SentenceBackground", typeof(RectTransform));
        var backgroundTransform = (RectTransform)backgroundObject.transform;
        backgroundTransform.SetParent(anchor, false);
        backgroundTransform.SetAsFirstSibling();
        backgroundTransform.pivot = new Vector2(0.5f, 0.5f);
        backgroundTransform.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundTransform.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundTransform.anchoredPosition = Vector2.zero;
        backgroundTransform.sizeDelta = (spriteSize != Vector2.zero ? spriteSize : sprite.rect.size) * scale;

        backgroundObject.AddComponent<Image>().sprite = sprite;

        sharedBackground = backgroundObject;
    }

    private List<WordCardView> PlaceByTemplate(SentenceTemplate template)
    {
        var slotCards = new List<WordCardView>();

        foreach (var element in template.elements)
        {
            var card = WordCardView.Create(container);

            if (element.type == SentenceElementType.FixedText)
            {
                card.SetFixedText(element.fixedText ?? string.Empty);
            }
            else
            {
                card.SetEmpty();
                slotCards.Add(card);
            }

            instances.Add(card);
        }

        return slotCards;
    }

    private void Clear()
    {
        foreach (var card in instances)
            if (card != null) Object.Destroy(card.gameObject);
        instances.Clear();

        if (sharedBackground != null)
        {
            Object.Destroy(sharedBackground);
            sharedBackground = null;
        }
    }
}
