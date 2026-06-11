using UnityEngine;

[CreateAssetMenu(menuName = "Novel/Character Key")]
public class CharacterKey : ScriptableObject
{
    [SerializeField] private string characterId;

    public string CharacterId => characterId;
}
