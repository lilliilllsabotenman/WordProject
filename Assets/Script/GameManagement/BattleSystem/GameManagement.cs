using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    [Header("ゲーム全体の進行データ")]
    [SerializeField] private GameSequenceData gameData;

    [Header("会話パート用NovelEventManager")]
    [SerializeField] private NovelEventManager novelEventManager;

    [Header("敵の文字表示・制限時間管理")]
    [SerializeField] private TextScaleAnimation textObject;

    [Header("反論組み立てカード")]
    [SerializeField] private Transform wordContainer;
    [SerializeField] private WordCardView wordCardPrefab;

    [Header("プレイヤーライフ管理")]
    [SerializeField] private LifeManager lifeManager;

    [Header("単語選択用マウスカーソル")]
    [SerializeField] private MouseControll mouse;

    private RoundManager roundManager;

    private void Awake()
    {
        var battleManager = new BattleManager(
            textObject,
            wordContainer,
            wordCardPrefab,
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
}