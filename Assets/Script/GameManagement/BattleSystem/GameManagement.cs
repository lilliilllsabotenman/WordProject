using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    [Header("ゲーム全体の進行データ")]
    [SerializeField] private GameSequenceData gameData;

    [Header("会話パート用NovelEventManager")]
    [SerializeField] private NovelEventManager novelEventManager;

    [Header("敵の文字表示・制限時間管理(ラウンド毎にAnchor位置へ生成するプレハブ)")]
    [SerializeField] private TextScaleAnimation textPrefab;
    [SerializeField] private Transform textAnchor;

    [Header("敵のスプライト表示")]
    [SerializeField] private SpriteRenderer enemySprite;

    [Header("反論組み立てカード")]
    [SerializeField] private Transform wordContainer;
    [SerializeField] private Transform sentenceLayoutAnchor;

    [Header("プレイヤーライフ管理")]
    [SerializeField] private LifeManager lifeManager;

    [Header("単語選択用マウスカーソル")]
    [SerializeField] private MouseControllBase mouse;

    [Header("BGM/SE再生用ハブ")]
    [SerializeField] private SoundHub soundHub;

    [Header("バトルBGM音量上限(クロスフェード時にこの値を超えない)")]
    [SerializeField, Range(0f, 1f)] private float battleBgmVolumeCap = 1f;

    [Header("クリア文字表示用")]
    [SerializeField] private TMPro.TextMeshProUGUI ClearText;

    [Header("正解/不正解の判定後、次の処理までの間")]
    [SerializeField] private float judgeDelay = 0.5f;

    [Header("判定SE")]
    [SerializeField] private SoundData completeSE;
    [SerializeField] private SoundData successSE;
    [SerializeField] private SoundData failureSE;

    private RoundManager roundManager;
    private BattleManager battleManager;

    private void Awake()
    {
        var animationManager = new NovelAnimationManager(
            FindObjectsByType<NovelAnimationObject>(FindObjectsSortMode.None));

        battleManager = new BattleManager(
            textPrefab,
            textAnchor,
            enemySprite,
            wordContainer,
            sentenceLayoutAnchor,
            lifeManager,
            mouse,
            new List<WordObject>(FindObjectsByType<WordObject>(FindObjectsSortMode.None)),
            new List<WordAsset>(Resources.LoadAll<WordAsset>("WordSettings/WordAssts")),
            animationManager,
            soundHub,
            completeSE,
            successSE,
            failureSE,
            ClearText,
            gameData.rounds.Count - 1,
            judgeDelay
        );

        roundManager = new RoundManager(
            gameData,
            novelEventManager,
            battleManager,
            soundHub,
            animationManager,
            Clear,
            GameOver,
            mouse,
            battleBgmVolumeCap);

        roundManager.InitializeCurrentRound();
    }

#if UNITY_EDITOR
    // デバッグ用: F1で現在のラウンドを強制的に正解扱いにする。
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            battleManager.DebugForceCorrect();
        }
    }
#endif

    private void Clear()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("End");
    }

    private void GameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BeginPlay");
    }
}