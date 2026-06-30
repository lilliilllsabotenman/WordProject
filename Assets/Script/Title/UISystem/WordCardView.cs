using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordCardView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;

    public void SetWord(WordAsset word, PlayerTextSpriteData spriteData = null)
    {
        label.text = word.DisplayText;
        ApplySprite(spriteData);
    }

    public void SetFixedText(string text, PlayerTextSpriteData spriteData = null)
    {
        label.text = text;
        ApplySprite(spriteData);
    }

    public void SetEmpty()
    {
        label.text = "■";
        if (background != null) background.sprite = null;
    }

    private void ApplySprite(PlayerTextSpriteData spriteData)
    {
        if (background == null) return;
        if (spriteData != null) background.sprite = spriteData.sprite;
    }
}
