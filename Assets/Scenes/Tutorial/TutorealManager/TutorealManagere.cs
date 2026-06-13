using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TutorealManagere : MonoBehaviour
{
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

    private RoundManager roundManager;

    private void Awake()
    {
        var battleManager = new BattleManager(
            textObject,
            sentenceText,
            lifeManager,
            mouse,
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

    void Start()
    {
        mouse.IsControll = false;
        tutoreal();
    }

    void tutoreal()
    {
        sentenceText.gameObject.SetActive(false);
        Debug.Log("1");
        novelEventManager.Play(novelGameSettings[0].DialogueCSV, tutoreal2);
    }

    void tutoreal2()
    {
        novelEventManager.Play(novelGameSettings[1].DialogueCSV, tutoreal3);
    }

    void tutoreal3()
    {
        novelEventManager.Play(novelGameSettings[2].DialogueCSV, tutoreal4);
    }

    void tutoreal4()
    {
        novelEventManager.Play(novelGameSettings[3].DialogueCSV, tutoreal5);
    }

    void tutoreal5()
    {
        novelEventManager.Play(novelGameSettings[4].DialogueCSV, tutoreal6);
    }

    void tutoreal6()
    {
        novelEventManager.Play(novelGameSettings[5].DialogueCSV, tutoreal7);
    }

    void tutoreal7()
    {
        novelEventManager.Play(novelGameSettings[6].DialogueCSV, tutoreal8);
    }

    void tutoreal8()
    {
        novelEventManager.Play(novelGameSettings[7].DialogueCSV, tutoreal9);
    }

    void tutoreal9()
    {
        novelEventManager.Play(novelGameSettings[8].DialogueCSV, tutoreal10);
    }

    void tutoreal10()
    {
        novelEventManager.Play(novelGameSettings[9].DialogueCSV, tutoreal11);
    }

    void tutoreal11()
    {
        novelEventManager.Play(novelGameSettings[10].DialogueCSV, tutoreal12);
    }

    void tutoreal12()
    {
        novelEventManager.Play(novelGameSettings[11].DialogueCSV, tutoreal13);
    }

    void tutoreal13()
    {
        novelEventManager.Play(novelGameSettings[12].DialogueCSV, tutoreal14);
    }

    void tutoreal14()
    {
        novelEventManager.Play(novelGameSettings[13].DialogueCSV, tutoreal15);
    }

    void tutoreal15()
    {
        novelEventManager.Play(novelGameSettings[14].DialogueCSV, tutoreal16);
    }

    void tutoreal16()
    {
        // チュートリアル終了処理
    }
}
