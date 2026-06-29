using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NovelGameDialogue;
using UnityEngine.UI;

[System.Serializable]
public class TutorealNovelData
{
    [Header("最初のやつ")]
    public NovelGameSettings a;

    [Header("ここで文字スロット表示")]


    [Header("文字強調")]

    [Header("レスだ抜き解説２")]
    public NovelGameSettings c;

    [Header("レスだぬきブちぎれ")]
    public NovelGameSettings d;

    [Header("レスだ抜きほめちぎり")]
    public NovelGameSettings e;

    [Header("二文字目")]
    public NovelGameSettings f;

    [Header("に文字とも正解")]
    public NovelGameSettings g;

    [Header("演出")]

    [Header("最後の方の会話")]
    public NovelGameSettings h;
}

public class TutorealManagere : MonoBehaviour
{
    [Header("会話手順")]
    [SerializeField] private TutorealNovelData tutorealData;

    [Header("ゲーム設定")]
    [SerializeField] private GameSequenceData gameData; 

    [Header("会話データ設定")]
    [SerializeField] private List<NovelGameSettings> novelGameSettings;

    [Header("会話パート用NovelEventManager")]
    [SerializeField] private NovelEventManager novelEventManager;

    [Header("敵の文字表示・制限時間管理")]
    [SerializeField] private TextScaleAnimation textObject;

    [Header("反論組み立て用テキスト")]
    [SerializeField] private TextMeshProUGUI sentenceText;

    [Header("プレイヤーライフ管理")]
    [SerializeField] private LifeManager lifeManager;

    [Header("単語選択用マウスカーソル")]
    [SerializeField] private MouseControll mouse;

    [Header("正解用WordAssets")]
    [SerializeField] private WordAsset Answer_1;

    [Header("正解用WordAssets")]
    [SerializeField] private WordAsset Answer_2;

    [Header("トーン♡")]
    [SerializeField] private Image t1;
    
    private RoundManager roundManager;

    private readonly ViewUIText viewUIText;
    private readonly WordManager wordManager;

    private TextAsset DialogueCSV;
    private TextDialogueOption textOption;

    private WordAsset Answer;

    private bool bAnswer_1 = false;
    private bool bAnswer_2 = false;
    private bool GetWord = false;

    private void Awake()
    {
    }

    private void Clear()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("End");
    }

    private void GameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BeginPlay");
    }

    private void Start()
    {
        _ = Tutoreal();
    }

    
    private async Task Tutoreal()//Todoこれをループにまとめられると思うのでそうしろ！！演出含め！！これ書いたあほ間抜けは重罪
    {


        await novelEventManager.PlayAsync(tutorealData.a);

        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));

        t1.gameObject.SetActive(false);       
        

        while (true)
        {
            Color color = sentenceText.color;

            color.a += (1f - color.a) / 60f;

            if (color.a >= 0.95f)
            {
                color.a = 1f;
                sentenceText.color = color;
                break;
            }

            sentenceText.color = color;

            await Task.Yield();
        }
        
        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));

        t1.gameObject.SetActive(true); 
        t1.transform.SetSiblingIndex(7); 

        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));

        await novelEventManager.PlayAsync(tutorealData.c);

        
        WordObject[] words = UnityEngine.Object.FindObjectsByType<WordObject>(FindObjectsSortMode.None);
        List<GameObject> wObj = new ();
        foreach(WordObject w in words)
        {
            wObj.Add(w.gameObject);
        }

        foreach(GameObject w in wObj)
        {
            w.SetActive(true);
        }


        Answer = Answer_1;
        mouse.MouseSubscrive(JudgmentWord);
        
        await MassageExecuter.WaitUntilAsync(() => GetWord);

        while(!bAnswer_1)
        {
            foreach(GameObject w in wObj)
            {
                w.SetActive(true);
            }

            await novelEventManager.PlayAsync(tutorealData.d);

            await MassageExecuter.WaitUntilAsync(() => GetWord);
            GetWord = false;
        }

        // t1.gameObject.SetActive(false); 
        t1.transform.SetSiblingIndex(9); 

        await novelEventManager.PlayAsync(tutorealData.e);

        GameObject aaa = null;

        foreach(GameObject w in wObj)
        {
            if(!w.activeSelf)
            {
                aaa = w;
            }
        }

        wObj.Remove(aaa);

        await MassageExecuter.WaitUntilAsync(() => Input.GetMouseButtonDown(0));

        await novelEventManager.PlayAsync(tutorealData.f);

        Answer = Answer_2;

        t1.gameObject.SetActive(false); 

        await MassageExecuter.WaitUntilAsync(() => GetWord);

        while(!bAnswer_2)
        {
            foreach(GameObject w in wObj)
            {
                w.SetActive(true);
            }

            await novelEventManager.PlayAsync(tutorealData.d);

            await MassageExecuter.WaitUntilAsync(() => GetWord);
            GetWord = false;
        }

        t1.gameObject.SetActive(true); 
        await novelEventManager.PlayAsync(tutorealData.g);

        //演出

        await novelEventManager.PlayAsync(tutorealData.h);
    }

    private void JudgmentWord(WordAsset word)
    {
        GetWord = true;
        if(word == Answer) 
        {
            if(Answer == Answer_1) bAnswer_1 = true;
            if(Answer == Answer_2) bAnswer_2 = true;
        }
    }
}