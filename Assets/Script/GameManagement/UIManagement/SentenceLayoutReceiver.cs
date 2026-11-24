using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SentenceLayoutReceiver : MonoBehaviour
{
    [SerializeField] private RectTransform anchor;
    [SerializeField] private float fontSize = 24f;
    [SerializeField] private Color textColor = Color.black;

    private readonly List<GameObject> instances = new();
    private readonly List<TextMeshProUGUI> slotLabels = new();
    private int nextSlotIndex;

    public void Initialize(SentenceLayoutData data)
    {
        Clear();

        if (data == null) return;

        foreach (var entry in data.entries)
        {
            var cardObject = new GameObject("SentenceCard", typeof(RectTransform));
            var cardRt = (RectTransform)cardObject.transform;
            cardRt.SetParent(anchor != null ? anchor : transform, false);
            cardRt.anchoredPosition = entry.position;
            cardRt.sizeDelta = entry.size;

            var background = cardObject.AddComponent<Image>();
            var sprite = entry.spriteData != null ? entry.spriteData.sprite : data.sprite;
            background.sprite = sprite;
            background.enabled = sprite != null;

            var labelObject = new GameObject("Label", typeof(RectTransform));
            var labelRt = (RectTransform)labelObject.transform;
            labelRt.SetParent(cardRt, false);
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.color = textColor;
            label.alignment = TextAlignmentOptions.Center;

            if (entry.isSlot)
            {
                label.text = string.Empty;
                slotLabels.Add(label);
            }
            else
            {
                label.text = entry.displayText;
            }

            instances.Add(cardObject);
        }

        nextSlotIndex = 0;
    }

    public void AddWord(string word)
    {
        if (nextSlotIndex >= slotLabels.Count) return;

        slotLabels[nextSlotIndex].text = word;
        nextSlotIndex++;
    }

    public void Clear()
    {
        foreach (var instance in instances)
            if (instance != null) Destroy(instance);

        instances.Clear();
        slotLabels.Clear();
        nextSlotIndex = 0;
    }
}