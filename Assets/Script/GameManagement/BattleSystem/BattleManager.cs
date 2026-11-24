using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MyTween;

public class BattleManager
{
    private readonly TextScaleAnimation textPrefab;
    private readonly Transform textAnchor;
    private TextScaleAnimation currentTextObject;
    private readonly SpriteRenderer enemySpriteRenderer;
    private readonly LifeManager lifeManager;
    private readonly MouseControllBase mouse;
    private readonly NovelAnimationManager animationManager;
    private readonly ISoundPlayer soundPlayer;
    private readonly SoundData completeSE;
    private readonly SoundData successSE;
    private readonly SoundData failureSE;
    private readonly TMPro.TextMeshProUGUI clearText;
    private readonly float judgeDelay;

    private readonly SentenceJudgement judgement = new();

    private readonly WordSlot wordSlot;
    private readonly SentenceCardPlacer placer;
    private readonly WordSlotFiller filler;
    private readonly WordManager wordManager;

    private RoundData roundData;

    private Action battleFinished;
    private Action gameOver;

    private int MaximRoundIndex;
    private int MyRoundIndex;

    public BattleManager(
        TextScaleAnimation textPrefab,
        Transform textAnchor,
        SpriteRenderer enemySpriteRenderer,
        Transform wordContainer,
        Transform sentenceLayoutAnchor,
        LifeManager lifeManager,
        MouseControllBase mouse,
        List<WordObject> wordObjects,
        List<WordAsset> wordPool,
        NovelAnimationManager animationManager,
        ISoundPlayer soundPlayer,
        SoundData completeSE,
        SoundData successSE,
        SoundData failureSE,
        TMPro.TextMeshProUGUI clearText,
        int maximRoundIndex,
        float judgeDelay = 0.5f
    )
    {
        this.textPrefab = textPrefab;
        this.textAnchor = textAnchor;
        this.enemySpriteRenderer = enemySpriteRenderer;
        this.lifeManager = lifeManager;
        this.mouse = mouse;
        this.animationManager = animationManager;
        this.soundPlayer = soundPlayer;
        this.completeSE = completeSE;
        this.successSE = successSE;
        this.failureSE = failureSE;
        this.clearText = clearText;
        this.judgeDelay = judgeDelay;

        this.MaximRoundIndex = maximRoundIndex;

        placer = new SentenceCardPlacer(wordContainer, sentenceLayoutAnchor);
        filler = new WordSlotFiller();

        wordSlot = new WordSlot(IsFulled, filler, PlayWordPlacedSound);

        wordManager = new WordManager(wordObjects, wordPool);

        if(mouse == null) return;
        mouse.MouseSubscrive(GetWordAsset);
    }

    public void InitializeBattle(
        RoundData data,
        Action battleFinished,
        Action gameOver,
        int MyRoundIndex)
    {
        Debug.Log("Mouse true");
        mouse.IsControll = true;
        roundData = data;

        if (currentTextObject != null)
        {
            UnityEngine.Object.Destroy(currentTextObject.gameObject);
        }
        currentTextObject = UnityEngine.Object.Instantiate(textPrefab, textAnchor.position, textAnchor.rotation);

        ReactivateWordObjects();
        InitializeClearText(data);

        this.battleFinished = battleFinished;
        this.gameOver = gameOver;

        lifeManager.Initialize();
        lifeManager.Subscribe(GameOver);

        judgement.SetAnswer(roundData.enemyData.sentenceTemplate);
        PlaceAndInitializeSlots();
        currentTextObject.InitializeEnemyText(roundData.enemyData, GameOver);
        wordManager.Initialize(roundData.enemyData.sentenceTemplate);

        this.MyRoundIndex = MyRoundIndex;
    }

    // 会話パート(CSV)再生終了を目印に、呼び出し側(RoundManager)から明示的に呼ぶ。
    public void SetEnemySprite(Sprite sprite)
    {
        if (enemySpriteRenderer == null) return;

        enemySpriteRenderer.sprite = sprite;
        enemySpriteRenderer.enabled = sprite != null;
    }

    public void DeactivateBattleObjects()
    {
        wordManager.DeactivateAll();
        // 破砕演出で既にDestroy済みの場合があるためnullチェックする。
        currentTextObject?.isFinished();
    }

    /// <summary>
    /// 呼び出し側からノベル終了後にバトル用WordObject群を復帰させるための仲介API。
    /// </summary>
    public void ReactivateWordObjects()
    {
        wordManager.ActivateObjects();
    }

    private void Incorrect()
    {
        PlaceAndInitializeSlots();

        wordManager.Initialize(roundData.enemyData.sentenceTemplate);

        lifeManager.DecreaseLife();
    }

    private void PlaceAndInitializeSlots()
    {
        var slotCards = placer.Place(roundData.enemyData.sentenceTemplate, roundData.enemyData.sentenceLayoutData);
        wordSlot.Initialize(roundData.enemyData.sentenceTemplate, slotCards);
    }

    private void PlayWordPlacedSound()
    {
        soundPlayer?.Play(completeSE);
    }

    private async void IsFulled(List<WordAsset> words)
    {
        bool correct = judgement.Judgement(words);

        await ResolveJudgement(correct);
    }

    // デバッグ用: 単語の内容に関係なく強制的に正解扱いにする。
    public void DebugForceCorrect()
    {
        _ = ResolveJudgement(true);
    }

    private async Task ResolveJudgement(bool correct)
    {
        await TweenActions.AnimateAsync(judgeDelay, null, _ => { });

        if (correct)
        {
            mouse.IsControll = false;

            if (roundData.enemyData.correctAnimations != null)
            {
                await roundData.enemyData.correctAnimations.PlayAsync(animationManager, autoAdvance: true);
            }

            soundPlayer?.Play(successSE);

            ShatterCurrentTextObject();

            await RevealClearTextAsync();

            battleFinished?.Invoke();
            Incorrect();
            return;
        }

        soundPlayer?.Play(failureSE);
        Incorrect();
    }

    private const float ShatterForce = 15f;

    /// <summary>
    /// 正解時に敵の文字オブジェクトを破砕する。実行後currentTextObjectはDestroy済みになる。
    /// </summary>
    private void ShatterCurrentTextObject()
    {
        if (currentTextObject == null) return;

        GameObject target = currentTextObject.gameObject;
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        Vector3 center = target.transform.position;

        // 現在のSprite(ラウンドごとに変わるenemyData.enemyText)に合わせてコライダーを都度計算する。
        BoxCollider2D box = target.AddComponent<BoxCollider2D>();
        box.size = sr.sprite.bounds.size;

        Explodable explodable = target.AddComponent<Explodable>();
        explodable.allowRuntimeFragmentation = true;
        explodable.extraPoints = 5;
        explodable.explode();

        foreach (GameObject frag in explodable.fragments)
        {
            Rigidbody2D rb = frag.GetComponent<Rigidbody2D>();
            Vector2 dir = ((Vector2)frag.transform.position - (Vector2)center).normalized;
            if (dir == Vector2.zero) dir = UnityEngine.Random.insideUnitCircle.normalized;
            rb.AddForce(dir * ShatterForce, ForceMode2D.Impulse);
        }

        currentTextObject = null;
    }

    private void InitializeClearText(RoundData data)
    {
        if (clearText == null) return;

        clearText.alpha = 0f;
        clearText.text = data.clearText;
    }

    /// <summary>
    /// 正解時に、既存のMyTween Fadeインフラを使ってClearTextをフェード表示する。
    /// </summary>
    private async Task RevealClearTextAsync(float duration = 1f)
    {
        if (clearText == null) return;

        var fadeData = new FadeRuntimeData
        {
            Target = new TextMeshProUGUIAdapter(clearText),
            Start = clearText.alpha,
            End = 1f,
            Duration = duration
        };
        await new FadeRuntimeCommand(fadeData).ExecuteAsync();
    }

    private void GetWordAsset(WordAsset word)
    {
        wordSlot.AddWord(word);
    }

    private void GameOver()
    {
        if(MyRoundIndex == MaximRoundIndex) return;
        gameOver?.Invoke();
    }
}
