using UnityEngine;

[CreateAssetMenu(menuName = "Novel/Character Key")]
public class CharacterKey : ScriptableObject
{
    [SerializeField] private string characterId;
    [SerializeField] private string displayName;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
}
