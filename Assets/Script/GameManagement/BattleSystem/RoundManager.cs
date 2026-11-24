using System;
using UnityEngine;
using MyTween;

[Serializable]
public class RoundManager
{
    private readonly GameSequenceData gameData;
    private readonly NovelEventManager novelEventManager;
    private readonly BattleManager battleManager;
    private readonly ISoundPlayer soundPlayer;
    private readonly NovelAnimationManager animationManager;

    private readonly Action clear;

    private readonly MouseControllBase mouseControlBase;
    private readonly Action gameOver;
    private readonly float battleBgmVolumeCap;

    private int dataIndex;

    public RoundManager(
        GameSequenceData gameData,
        NovelEventManager novelEventManager,
        BattleManager battleManager,
        ISoundPlayer soundPlayer,
        NovelAnimationManager animationManager,
        Action clear,
        Action gameOver,
        MouseControllBase mouseControllBase,
        float battleBgmVolumeCap = 1f)
    {
        this.gameData = gameData;
        this.novelEventManager = novelEventManager;
        this.battleManager = battleManager;
        this.soundPlayer = soundPlayer;
        this.animationManager = animationManager;

        this.clear = clear;
        this.gameOver = gameOver;
        this.mouseControlBase = mouseControlBase;
        this.battleBgmVolumeCap = battleBgmVolumeCap;
    }

    public async void InitializeCurrentRound()
    {
        if (gameData == null || !gameData.isCanPlaing())
            return;

        RoundData round = gameData.rounds[dataIndex];

        if (round.enemyData.preRoundAnimations != null)
        {
            await round.enemyData.preRoundAnimations.PlayAsync(animationManager);
        }

        if (round.preBattleDialogueCSV != null)
        {
            novelEventManager.Play(round.preBattleDialogueCSV, () => StartBattle(round));
            return;
        }

        StartBattle(round);
    }

    private void StartBattle(RoundData round)
    {
        // 会話パート(CSV)再生終了を目印にここでスプライトを切り替える。
        // 初回ラウンドは会話パートが無いため、ラウンド開始時にここで反映する。
        battleManager.SetEnemySprite(round.enemyData.enemySprite);
        if (soundPlayer == null)
        {
            Debug.LogWarning("RoundManager: soundPlayerが未設定です。BGM再生をスキップします。");
        }
        else
        {
            soundPlayer.Play(round.enemyData.battleBGM, battleBgmVolumeCap);
        }
        battleManager.InitializeBattle(round, BattleFinished, gameOver, dataIndex);
    }

    private void BattleFinished()
    {
        RoundData round = gameData.rounds[dataIndex];

        if (!round.skipDialogue)
        {
            battleManager.DeactivateBattleObjects();

            if(mouseControlBase != null)
            {

            }
            novelEventManager.Play(round.dialogueCSV, NovelFinished);

            return;
        }

        NovelFinished();
    }

    private async void NovelFinished()
    {
        RoundData round = gameData.rounds[dataIndex];

        if (round.enemyData.postDialogueAnimations != null)
        {
            await round.enemyData.postDialogueAnimations.PlayAsync(animationManager);
        }

        if (round.roundTransitionDelay > 0f)
        {
            await TweenActions.AnimateAsync(round.roundTransitionDelay, null, _ => { });
        }

        dataIndex++;

        if (dataIndex >= gameData.rounds.Count)
        {
            clear?.Invoke();
            return;
        }

        InitializeCurrentRound();
    }
}


