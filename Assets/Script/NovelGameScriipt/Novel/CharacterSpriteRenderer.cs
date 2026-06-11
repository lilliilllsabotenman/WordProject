using UnityEngine;

public class CharacterSpriteRenderer : MonoBehaviour
{
    [SerializeField] private CharacterKey characterKey;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public CharacterKey GetCharacterKey()
    {
        return characterKey;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
