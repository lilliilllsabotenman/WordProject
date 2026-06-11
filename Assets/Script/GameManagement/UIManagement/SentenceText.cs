using System.Text;
using TMPro;
using UnityEngine;

public class SentenceText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sentenceText;

    private SentenceTemplate currentTemplate;
    private WordSlotState currentState;

    public void Initialize(SentenceTemplate template, WordSlotState state)
    {
        currentTemplate = template;

        if (currentState != null)
        {
            currentState.OnChanged -= HandleSentenceChanged;
        }

        currentState = state;

        if (currentState != null)
        {
            currentState.OnChanged += HandleSentenceChanged;
        }

        Render();
    }

    private void HandleSentenceChanged()
    {
        Render();
    }

    private void Render()
    {
        if (sentenceText == null)
        {
            return;
        }

        if (currentTemplate == null || currentTemplate.elements == null)
        {
            sentenceText.text = string.Empty;
            return;
        }

        StringBuilder builder = new();
        int slotIndex = 0;

        foreach (var element in currentTemplate.elements)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            if (element.type == SentenceElementType.FixedText)
            {
                builder.Append(element.fixedText ?? string.Empty);
                continue;
            }

            if (!element.IsValidInputSlot())
            {
                continue;
            }

            WordAsset answer = null;
            if (currentState != null && slotIndex < currentState.CurrentAnswers.Count)
            {
                answer = currentState.CurrentAnswers[slotIndex];
            }

            builder.Append(answer != null ? answer.DisplayText : " ");
            slotIndex++;
        }

        sentenceText.text = builder.ToString();
    }

    private void OnDestroy()
    {
        if (currentState != null)
        {
            currentState.OnChanged -= HandleSentenceChanged;
        }
    }
}
