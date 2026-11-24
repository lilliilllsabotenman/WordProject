using System.Collections.Generic;
using System;

public class WordSlot 
{
    private SentenceTemplate AnswerSentence;
    private List<WordAsset> AnswerWordList = new();
    private List<WordAsset> words = new();
    private WordSlotFiller filler;

    private event Action<List<WordAsset>> isFulled;
    private event Action wordPlaced;


    public WordSlot(
        Action<List<WordAsset>> action,
        WordSlotFiller filler,
        Action onWordPlaced = null)
    {
        isFulled += action;
        this.filler = filler;
        if (onWordPlaced != null) wordPlaced += onWordPlaced;
    }

    public void Initialize(SentenceTemplate answer, List<WordCardView> slotCards)
    {
        AnswerWordList.Clear();
        AnswerSentence = answer;
        AnswerWordList = answer.GetWordAssetsList();

        words.Clear();

        filler.Initialize(slotCards);
    }

    public void AddWord(WordAsset WordAsset)
    {
        words.Add(WordAsset);
        filler.AddWord(WordAsset);

        UnityEngine.Debug.Log(words.Count);

        wordPlaced?.Invoke();

        if(words.Count >= AnswerWordList.Count)//設定されたスロットが全部埋まったか確認
            isFulled?.Invoke(words);

    }
}
