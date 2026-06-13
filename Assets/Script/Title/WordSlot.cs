using System.Collections.Generic;
using System;

public class WordSlot 
{
    private SentenceTemplate AnswerSentence;
    private List<WordAsset> AnswerWordList = new();
    private List<WordAsset> words = new();
    private ViewUIText viewUIText;

    private event Action<List<WordAsset>> isFulled;
    

    public WordSlot(
        Action<List<WordAsset>> action,
        ViewUIText viewUIText)
    {
        isFulled += action;
        this.viewUIText = viewUIText;
    }

    public void Initialize(SentenceTemplate answer)
    {
        AnswerSentence = answer;
        AnswerWordList = answer.GetWordAssetsList();
        AnswerWordList.Clear();
        words.Clear();

        viewUIText.TextInitialize(AnswerSentence);
    }

    public void AddWord(WordAsset WordAsset)
    {
        words.Add(WordAsset);
        viewUIText.AddWord(WordAsset);

        UnityEngine.Debug.Log(words.Count);

        if(words.Count >= AnswerWordList.Count)//設定されたスロットが全部埋まったか確認
            isFulled?.Invoke(words);

    }
}
