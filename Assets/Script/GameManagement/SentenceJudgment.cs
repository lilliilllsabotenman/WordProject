using System.Collections.Generic;
using System.Linq;

public class SentenceJudgment
{
    private List<WordAsset> correctAnswers = new();

    public SentenceJudgment(SentenceTemplate template)
    {
        Initialize(template);
    }

    public bool JudgeSentence(IReadOnlyList<WordAsset> currentAnswers)//正解判定の正体
    {
        if (currentAnswers == null)
        {
            return false;
        }

        if (correctAnswers.Count != currentAnswers.Count)
        {
            return false;
        }

        for (int i = 0; i < correctAnswers.Count; i++)
        {
            if (correctAnswers[i] != currentAnswers[i])
            {
                return false;
            }
        }

        return true;
    }

    public void Initialize(SentenceTemplate template)
    {
        correctAnswers = template == null
            ? new List<WordAsset>()
            : template.elements
                .Where(e => e.IsValidInputSlot())
                .Select(e => e.GetSlotWord())
                .ToList();
    }
}
