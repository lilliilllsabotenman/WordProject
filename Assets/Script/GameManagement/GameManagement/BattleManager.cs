using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class BattleManager
{
    [Header("UI")]
    [SerializeField] private TextScaleAnimation textObject;
    [SerializeField] private SentenceText sentenceText;

    [Header("ライフ")]
    [SerializeField] private LifeManager lifeManager;

    private WordSlotState wordSlotState;
    private WordManager wordManager;
    private SentenceJudgment sentenceJudgment;
    private readonly TimeManagement timeManagement = new();
    private RoundData roundData;

    public BattleManager()
    {

    }

    public void InitializeBattle(RoundData _data, Action action)
    {
        roundData = _data;

        if (roundData == null || roundData.enemyData == null)
        {
            return;
        }


        EnemyData data = roundData.enemyData;
        SentenceTemplate template = data.sentenceTemplate;

        if (template == null || template.elements == null)
        {
            return;
        }
        

        int slotCount = template.elements.Count(e => e.IsValidInputSlot());
        if (slotCount <= 0)
        {
            Debug.LogWarning("GameManagement: sentenceTemplate has no valid input slots.");
            return;
        }

        if (wordSlotState != null)
        {
            wordSlotState.OnChanged -= action;
        }

        wordSlotState = new WordSlotState(slotCount);
        wordSlotState.OnChanged += action;

        sentenceJudgment.Initialize(template);

        sentenceText?.Initialize(template, wordSlotState);
        timeManagement.Initialize(data.limitTime);
        wordManager.Initialize(template);

        if (textObject != null)
        {
            textObject.SetText(data.enemyText);
        }

        if (sentenceText != null)
        {
            sentenceText.gameObject.SetActive(false);
            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("WordObject")) obj.SetActive(false);
        }
    }


    public void OnUpdate(float DeltaTime)
    {
        if (timeManagement.OnUpdate(DeltaTime))
        {
            //ゲームオーバー処理
        }
    }
}
