using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Sequence Data")]
public class GameSequenceData : ScriptableObject
{
    [Header("Round Data")]
    public List<RoundData> rounds = new();

    public bool isCanPlaing()
    {
        if(this.rounds == null || this.rounds.Count <= 0) return false;

        return true;
    }
}

[System.Serializable]
public class RoundData
{
    [Header("Enemy Data")]
    public EnemyData enemyData;

    [Header("Dialogue CSV")]
    public TextAsset dialogueCSV;

    [Header("Skip Dialogue")]
    public bool skipDialogue;
}
