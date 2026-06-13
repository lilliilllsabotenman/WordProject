using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TitleSceneManagement : MonoBehaviour
{
    [Header("タイトルシーン設定")]
    [SerializeField] private TitleSceneData titleSceneData;

    [Header("文字スロット")]
    [SerializeField] TMPro.TextMeshProUGUI Text;

    [Header("マウスオブジェクト")]
    [SerializeField] private MouseControll mosue;

    private SentenceJudgement Judgement = new();
    private WordSlot wordSlot;
    private ViewUIText viewUIText;
    private WordManager wordManager;

    private void Start()
    {
        if(mosue == null) return;

        viewUIText = new ViewUIText(Text);

        wordSlot = new WordSlot(
            isFulled,
            viewUIText);

        wordManager = new WordManager(
            new List<WordObject>(UnityEngine.Object.FindObjectsByType<WordObject>(FindObjectsSortMode.None)),
            new List<WordAsset>(Resources.LoadAll<WordAsset>("TitleWord")));
        
               
        if(titleSceneData.SceneData.Count == 0) return;

        mosue.MouseSubscrive(GetWordAsset);
        SentenceInitialized();
        wordManager.Initialize(titleSceneData.SceneData[0].SentenceData);
    }

    private void GetWordAsset(WordAsset word)
    {
        Debug.Log("GetWordAssets");
        if(wordSlot == null) return;

        wordSlot.AddWord(word);
    }

    private void isFulled(List<WordAsset> words)
    {
        foreach(TitleActionData data in titleSceneData.SceneData)
        {
            if(Judgement.Judgement(data.SentenceData, words))
            {
                ChangeScene(data.sceneName);
                return;
            }
        }

        SentenceInitialized();
    }

    private void SentenceInitialized()
    {   
        if(titleSceneData.SceneData.Count == 0) return;
        wordSlot.Initialize(titleSceneData.SceneData[0].SentenceData);//どうせ全部一緒の内容で行けるようにしてあるから０番直指定
    }

    private void ChangeScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}