using System;
using UnityEngine;

[Serializable]
public class RoundManager
{
    private readonly GameSequenceData gameData;
    private readonly NovelEventManager novelEventManager;
    private readonly BattleManager battleManager;

    private readonly Action clear;
    private readonly Action gameOver;

    private int dataIndex;

    public RoundManager(
        GameSequenceData gameData,
        NovelEventManager novelEventManager,
        BattleManager battleManager,
        Action clear,
        Action gameOver)
    {
        this.gameData = gameData;
        this.novelEventManager = novelEventManager;
        this.battleManager = battleManager;

        this.clear = clear;
        this.gameOver = gameOver;
    }

    public void InitializeCurrentRound()
    {
        if (gameData == null || !gameData.isCanPlaing())
            return;

        battleManager.InitializeBattle(gameData.rounds[dataIndex], BattleFinished, gameOver);
    }

    private void BattleFinished()
    {
        RoundData round = gameData.rounds[dataIndex];

        if (!round.skipDialogue)
        {
            novelEventManager.Play(round.dialogueCSV, NovelFinished);

            return;
        }

        NovelFinished();
    }

    private void NovelFinished()
    {
        dataIndex++;

        if (dataIndex >= gameData.rounds.Count)
        {
            clear?.Invoke();
            return;
        }

        InitializeCurrentRound();
    }
}