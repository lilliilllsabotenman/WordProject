using UnityEngine;
using System.Collections.Generic;

public class ViewUIText
{
    private Transform container;
    private WordCardView cardPrefab;
    private SentenceTemplate template;
    private List<WordAsset> words = new();
    private List<WordCardView> slotCards = new();

    public ViewUIText(Transform container, WordCardView cardPrefab)
    {
        this.container = container;
        this.cardPrefab = cardPrefab;
    }

    public void TextInitialize(SentenceTemplate template)
    {
        this.template = template;
        words.Clear();
        ClearCards();
        BuildCards();
    }

    private void ClearCards()
    {
        foreach (Transform child in container)
            Object.Destroy(child.gameObject);
        slotCards.Clear();
    }

    private void BuildCards()
    {
        if (container == null) { Debug.LogError("ViewUIText: コンテナの参照がありません"); return; }
        if (cardPrefab == null) { Debug.LogError("ViewUIText: カードプレハブの参照がありません"); return; }

        foreach (var element in template.elements)
        {
            var card = Object.Instantiate(cardPrefab, container);
            if (element.type == SentenceElementType.FixedText)
            {
                card.SetFixedText(element.fixedText ?? string.Empty);
            }
            else
            {
                card.SetEmpty();
                slotCards.Add(card);
            }
        }
    }

    public void AddWord(WordAsset word)
    {
        int slotIndex = words.Count;
        words.Add(word);

        if (slotIndex < slotCards.Count)
            slotCards[slotIndex].SetWord(word);
    }
}