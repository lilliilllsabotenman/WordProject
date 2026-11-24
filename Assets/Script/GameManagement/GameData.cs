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
    public Sprite enemyText;

    [FormerlySerializedAs("answerSentence")]
    public SentenceTemplate sentenceTemplate;

    [FormerlySerializedAs("TemplateData")]
    public SentenceLayoutData sentenceLayoutData;

    public float limitTime = 30f;

    public Sprite enemySprite;

    public SoundData battleBGM;

    [Header("① ラウンド開始前に再生するアニメーション")]
    public DataSequence preRoundAnimations;

    [Header("② 正解直後に再生するアニメーション")]
    public DataSequence correctAnimations;

    [Header("③ CSV会話パート終了後に再生するアニメーション")]
    public DataSequence postDialogueAnimations;
}
