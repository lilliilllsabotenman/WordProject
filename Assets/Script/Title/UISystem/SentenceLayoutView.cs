using System.Collections.Generic;
using UnityEngine;

public class SentenceLayoutView
{
    private readonly List<WordCardView> instances = new();
    private readonly List<WordCardView> slotCards = new();

    public void Initialize(RectTransform anchor, SentenceLayoutData data, WordCardView prefab)
    {
        Clear();

        foreach (var entry in data.entries)
        {
            var card = Object.Instantiate(prefab, anchor);
            var rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = entry.position;
            rt.sizeDelta = entry.size;

            if (entry.isSlot)
            {
                card.SetEmpty();
                slotCards.Add(card);
            }
            else
            {
                card.SetFixedText(entry.displayText, entry.spriteData);
            }

            instances.Add(card);
        }
    }

    public void FillSlot(int slotIndex, WordAsset word)
    {
        if (slotIndex >= slotCards.Count) return;
        slotCards[slotIndex].SetWord(word);
    }

    public void Clear()
    {
        foreach (var card in instances)
            if (card != null) Object.Destroy(card.gameObject);
        instances.Clear();
        slotCards.Clear();
    }
}