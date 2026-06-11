using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "GameData")]
public class GameData : ScriptableObject
{
    public List<EnemyData> enemyData;
}

[System.Serializable]
public class EnemyData
{
    [FormerlySerializedAs("EnemyTextl")]
    public string enemyText;

    [FormerlySerializedAs("answerSentence")]
    public SentenceTemplate sentenceTemplate;

    public float limitTime = 30f;
}
