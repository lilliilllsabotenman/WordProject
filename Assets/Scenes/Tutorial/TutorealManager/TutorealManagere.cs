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

    [Header("正解用WordAssets")]
    [SerializeField] private WordAsset word_1;

    private MouseControll m = null;
    private RoundManager roundManager;

    private readonly ViewUIText viewUIText;
    private readonly WordManager wordManager;

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

    void Start()
    {
        mouse.IsControll = false;
        tutoreal();
    }

    void tutoreal()
    {
        sentenceText.gameObject.SetActive(false);
        Debug.Log(novelGameSettings[0].DialogueCSV);
        novelEventManager.Play(novelGameSettings[0].DialogueCSV, tutoreal2);
    }

    void tutoreal2()
    {
        sentenceText.gameObject.SetActive(true);
        novelEventManager.Play(novelGameSettings[1].DialogueCSV, tutoreal3);
    }

    void tutoreal3()
    {
        novelEventManager.Play(novelGameSettings[1].DialogueCSV, tutoreal3);
    }
}
