using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NovelGameDialogue;

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

    private MouseControll m = null;
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
        BattleManager battleManager = new BattleManager(
            textObject,
            sentenceText,
            lifeManager,
            m,
            new List<WordObject>(FindObjectsByType<WordObject>(FindObjectsSortMode.None)),
            new List<WordAsset>(Resources.LoadAll<WordAsset>("WordSettings/WordAssets"))
        );

        roundManager = new RoundManager(
            gameData,
            novelEventManager,
            battleManager,
            Clear,
            GameOver);

        roundManager.InitializeCurrentRound();
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


        await novelEventManager.PlayAsync(tutorealData.c);

        //文字強調

        Answer = Answer_1;
        mouse.MouseSubscrive(JudgmentWord);
        
        await MassageExecuter.WaitUntilAsync(() => GetWord);

        while(bAnswer_1)
        {
            await novelEventManager.PlayAsync(tutorealData.d);

            await MassageExecuter.WaitUntilAsync(() => GetWord);
            GetWord = false;
        }

        await novelEventManager.PlayAsync(tutorealData.e);


        Answer = Answer_2;

        await MassageExecuter.WaitUntilAsync(() => GetWord);

        while(bAnswer_2)
        {
            await novelEventManager.PlayAsync(tutorealData.f);

            await MassageExecuter.WaitUntilAsync(() => GetWord);
            GetWord = false;
        }

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
