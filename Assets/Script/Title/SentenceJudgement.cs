using UnityEngine;
using System.Collections.Generic;

public class SentenceJudgement
{
    SentenceTemplate AnswerSentence;
    List<WordAsset> answer = new();

    public void SetAnswer(SentenceTemplate Answer)
    {
        AnswerSentence = Answer;
    }

    public bool Judgement(List<WordAsset> words)
    {
        if(AnswerSentence == null) return false;

        answer = AnswerSentence.GetWordAssetsList();

        if(words.Count != answer.Count) return false;
        
        for(int i = 0; i < answer.Count; i++)
        {
            if(words[i] != answer[i]) return false;
        }

        return true;
    }

    public bool Judgement(SentenceTemplate Answer, List<WordAsset> words)
    {
        if(Answer == null) return false;

        answer = Answer.GetWordAssetsList();

        if(words.Count != answer.Count) return false;
        
        for(int i = 0; i < answer.Count; i++)
        {
            if(words[i] != answer[i]) return false;
        }

        return true;
    }
}