using UnityEngine;
using UnityEngine.UI;

public class CharacterSpriteRenderer : MonoBehaviour
{
    [SerializeField] private CharacterKey characterKey;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;

    public CharacterKey GetCharacterKey()
    {
        return characterKey;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public Image GetImage()
    {
        return image;
    }

    // SpriteRendererとImage、どちらが割り当てられていてもここで見た目を更新できる。
    public Sprite Sprite
    {
        get
        {
            if (spriteRenderer != null) return spriteRenderer.sprite;
            if (image != null) return image.sprite;
            return null;
        }
        set
        {
            if (spriteRenderer != null) spriteRenderer.sprite = value;
            if (image != null) image.sprite = value;
        }
    }

    public bool Visible
    {
        get
        {
            if (spriteRenderer != null) return spriteRenderer.enabled;
            if (image != null) return image.enabled;
            return false;
        }
        set
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = value;
                if (value) ForceOpaque(spriteRenderer);
            }

            if (image != null)
            {
                image.enabled = value;
                if (value) ForceOpaque(image);
            }
        }
    }

    // シーン側でアルファ0のまま放置されていても、表示するタイミングで強制的に不透明にする。
    private static void ForceOpaque(SpriteRenderer renderer)
    {
        Color c = renderer.color;
        c.a = 1f;
        renderer.color = c;
    }

    private static void ForceOpaque(Image image)
    {
        Color c = image.color;
        c.a = 1f;
        image.color = c;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
    }
}
