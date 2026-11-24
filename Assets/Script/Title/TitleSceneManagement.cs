using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using MyTween;

public class TitleSceneManagement : MonoBehaviour
{
    [Header("タイトルシーン設定")]
    [SerializeField] private TitleSceneData titleSceneData;

    [Header("文字スロット")]
    [SerializeField] private Transform wordContainer;

    [Header("カード表示用テキスト(選んだ順に入る)")]
    [SerializeField] private List<TextMeshProUGUI> sentenceSlots;

    [Header("マウスオブジェクト")]
    [SerializeField] private MouseControllBase mosue;

    [Header("判定後、次の処理までの間")]
    [SerializeField] private float judgeDelay = 0.5f;

    private SentenceJudgement Judgement = new();
    private WordSlot wordSlot;
    private SentenceCardPlacer placer;
    private WordSlotFiller filler;
    private WordManager wordManager;
    private int sentenceSlotIndex;

    private void Start()
    {
        if(mosue == null) return;

        placer = new SentenceCardPlacer(wordContainer);
        filler = new WordSlotFiller();

        wordSlot = new WordSlot(
            isFulled,
            filler);

        wordManager = new WordManager(
            new List<WordObject>(UnityEngine.Object.FindObjectsByType<WordObject>(FindObjectsSortMode.None)),
            new List<WordAsset>(Resources.LoadAll<WordAsset>("TitleWord")));
        
               
        if(titleSceneData.SceneData.Count == 0) return;

        mosue.MouseSubscrive(GetWordAsset);
        SentenceInitialized();
    }

    private void GetWordAsset(WordAsset word)
    {
        Debug.Log("GetWordAssets");
        if(wordSlot == null) return;

        if (sentenceSlots != null && sentenceSlotIndex < sentenceSlots.Count && sentenceSlots[sentenceSlotIndex] != null)
        {
            sentenceSlots[sentenceSlotIndex].text = word.DisplayText;
            sentenceSlotIndex++;
        }

        wordSlot.AddWord(word);
    }

    private async void isFulled(List<WordAsset> words)
    {
        TitleActionData? matched = null;

        foreach(TitleActionData data in titleSceneData.SceneData)
        {
            if(Judgement.Judgement(data.SentenceData, words))
            {
                matched = data;
                break;
            }
        }

        await TweenActions.AnimateAsync(judgeDelay, null, _ => { });

        if(matched.HasValue)
        {
            ChangeScene(matched.Value.sceneName);
            return;
        }

        SentenceInitialized();
    }

    private void SentenceInitialized()
    {
        if(titleSceneData.SceneData.Count == 0) return;

        sentenceSlotIndex = 0;
        if (sentenceSlots != null)
        {
            foreach (var slot in sentenceSlots)
            {
                if (slot != null) slot.text = string.Empty;
            }
        }

        var template = titleSceneData.SceneData[0].SentenceData;
        var slotCards = placer.Place(template, null);
        wordSlot.Initialize(template, slotCards);//どうせ全部一緒の内容で行けるようにしてあるから０番直指定
        wordManager.Initialize(template);
    }

    private void ChangeScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}