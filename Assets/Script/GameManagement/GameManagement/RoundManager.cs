using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class RoundManager
{
    [Header("Game Data Sequence")]
    [SerializeField] private GameSequenceData gameData;

    [Header("Dialogue System")]
    [SerializeField] private NovelEventManager novelEventManager;

    [Header("BattleSystem")]
    [SerializeField] private BattleManager battleManager = new BattleManager();

    
    
    private WordSlotState wordSlotState;
    private LifeManager lifeManager;

    private SentenceJudgment sentenceJudgment;

    private TextScaleAnimation textObject;

    private int dataIndex = 0;

    public void InitializeCurrentRound()
    {
        if (gameData == null || !gameData.isCanPlaing())
        {
            return;
        }

        if (dataIndex < 0 || dataIndex >= gameData.rounds.Count)//念のためのIndex初期化
        {
            dataIndex = 0;
        }

        battleManager.InitializeBattle(gameData.rounds[dataIndex], OnSentenceChanged);
        // lifeManager?.Subscribe(GameOver);
    }

    private void ProceedNextRound()
    {
        dataIndex++;

        if (gameData == null || gameData.rounds == null || dataIndex >= gameData.rounds.Count)
        {
            if (textObject != null)
            {
            }



            return;
        }

        InitializeCurrentRound();
    }

    private void OnSentenceChanged()
    {
        if (wordSlotState == null || !wordSlotState.IsFull())
        {
            return;
        }

        if (sentenceJudgment.JudgeSentence(wordSlotState.CurrentAnswers))
        {
            // OnRoundClear();
            return;
        }

        lifeManager?.DecreaseLife();
        wordSlotState.ClearAll();



        if (gameData.rounds == null)
        {
            return;
        }

        if (dataIndex < 0 || dataIndex >= gameData.rounds.Count)
        {
            return;
        }

        SentenceTemplate currentTemplate =
            gameData.rounds[dataIndex]
                .enemyData
                .sentenceTemplate;

        // if (currentTemplate != null)
        // {
        //     wordManager.Initialize(currentTemplate);
        // }
    }

    private void OnRoundClear()
    {
        Debug.Log("seikai");

        if (gameData == null || gameData.rounds == null || dataIndex >= gameData.rounds.Count)
        {
            return;
        }

        RoundData current = gameData.rounds[dataIndex];

        if (current.skipDialogue || current.dialogueCSV == null)
        {
            ProceedNextRound();
            return;
        }

        if (novelEventManager != null)
        {
            textObject.SetText("");
            ProceedNextRound();
            novelEventManager.Play(current.dialogueCSV);
           
        }
        else
        {
            ProceedNextRound();
        }
    }
}