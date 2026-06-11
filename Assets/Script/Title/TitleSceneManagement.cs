using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct TitleActionData
{
    public string sceneName;
    public SentenceTemplate SentenceData;
}

[Serializable]
public struct TitleSceneData
{
    [SerializeField]
    private List<TitleActionData> SceneData;

    private Dictionary<SentenceTemplate, string> sceneData;

    private void Initialize()
    {
        sceneData = new Dictionary<SentenceTemplate, string>();

        foreach (var data in SceneData)
        {
            if (string.IsNullOrWhiteSpace(data.sceneName))
            {
                Debug.LogError("TitleActionData の SceneName が未設定です。");
                return;
            }

            if (data.SentenceData == null)
            {
                Debug.LogError("TitleActionData の SentenceTemplate が未設定です。");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(data.sceneName))
            {
                Debug.LogError($"シーン '{data.sceneName}' が Build Settings に存在しません。");
                return;
            }

            sceneData[data.SentenceData] = data.sceneName;
        }
    }
}

public class TitleSceneManagement : MonoBehaviour
{
    [SerializeField] private TitleSceneData titleSceneData;

    
}