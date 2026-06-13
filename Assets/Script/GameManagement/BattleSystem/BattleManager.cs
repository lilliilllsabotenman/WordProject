using System;
using System.Collections.Generic;
using TMPro;

public class BattleManager
{
    private readonly TextScaleAnimation textObject;
    private readonly TextMeshProUGUI sentenceText;
    private readonly LifeManager lifeManager;
    private readonly MouseControll mouse;

    private readonly SentenceJudgement judgement = new();

    private readonly WordSlot wordSlot;
    private readonly ViewUIText viewUIText;
    private readonly WordManager wordManager;

    private RoundData roundData;

    private Action battleFinished;
    private Action gameOver;

    public BattleManager(
        TextScaleAnimation textObject,
        TextMeshProUGUI sentenceText,
        LifeManager lifeManager,
        MouseControll mouse,
        List<WordObject> wordObjects,
        List<WordAsset> wordPool)
    {
        this.textObject = textObject;
        this.sentenceText = sentenceText;
        this.lifeManager = lifeManager;
        this.mouse = mouse;

        viewUIText = new ViewUIText(sentenceText);

        wordSlot = new WordSlot(IsFulled, viewUIText);

        wordManager = new WordManager(wordObjects, wordPool);

        mouse.MouseSubscrive(GetWordAsset);
    }

    public void InitializeBattle(
        RoundData data,
        Action battleFinished,
        Action gameOver)
    {
        roundData = data;
        ReactivateWordObjects();

        this.battleFinished = battleFinished;
        this.gameOver = gameOver;

        lifeManager.Initialize();
        lifeManager.Subscribe(GameOver);

        judgement.SetAnswer(roundData.enemyData.sentenceTemplate);
        wordSlot.Initialize(roundData.enemyData.sentenceTemplate);
        textObject.InitializeEnemyText(roundData.enemyData,GameOver);
        wordManager.Initialize(roundData.enemyData.sentenceTemplate);
    }

    public void DeactivateBattleObjects()
    {
        wordManager.DeactivateAll();
        textObject.isFinished();
    }

    /// <summary>
    /// 呼び出し側からノベル終了後にバトル用WordObject群を復帰させるための仲介API。
    /// </summary>
    public void ReactivateWordObjects()
    {
        wordManager.ActivateObjects();
        textObject.isFinished();
        textObject.gameObject.SetActive(true);
    }

    private void Incorrect()
    {
        wordSlot.Initialize(roundData.enemyData.sentenceTemplate);

        wordManager.Initialize(roundData.enemyData.sentenceTemplate);

        lifeManager.DecreaseLife();
    }

    private void IsFulled(List<WordAsset> words)
    {
        UnityEngine.Debug.Log("IsFull");
        if (judgement.Judgement(words))
        {
            battleFinished?.Invoke();
            return;
        }

        Incorrect();
    }

    private void GetWordAsset(WordAsset word)
    {
        wordSlot.AddWord(word);
    }

    private void GameOver()
    {
        gameOver?.Invoke();
    }
}
