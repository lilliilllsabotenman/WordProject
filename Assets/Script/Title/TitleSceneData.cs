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
public class TitleSceneData
{
    [Header("シーンと文章の紐づけ")]
    public List<TitleActionData> SceneData;
}