using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    public WordSlotState sentenceState;
    public RoundManager roundManager;

    private WordManager wordManager;
    private SentenceJudgment sentenceJudgment;

    private void Awake()
    {
        sentenceJudgment = new SentenceJudgment(null);
        roundManager = new RoundManager();

        //ワードオブジェクトランダム配置システム初期化
        wordManager = new WordManager(
            new List<WordObject>(Object.FindObjectsByType<WordObject>(FindObjectsSortMode.None)),
            new List<WordAsset>(Resources.LoadAll<WordAsset>("WordSettings/WordAssets")));

        // if (novelEventManager != null)
        // {
        //     novelEventManager.OnDialogueFinished += Clear;
        // }

        roundManager.InitializeCurrentRound();
    }

    private void Update()
    {
        // if (novelEventManager != null && novelEventManager.IsPlaying)
        // {
        //     return;
        // }
    }

    public WordSlotState GetOrCreateSentenceState()
    {
        if (sentenceState == null)
        {
            roundManager.InitializeCurrentRound();
        }

        return sentenceState;
    }

    // private void OnDialogueFinished()
    // {
    //     ProceedNextRound();
    // }

    private void GameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BeginPlay");
    }

    private void Clear()
    {
         UnityEngine.SceneManagement.SceneManager.LoadScene("End");
    }
}
