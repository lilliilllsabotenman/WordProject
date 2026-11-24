using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordCardView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;

    private float baseFontSize = 24f;
    private float minFontSize = 24f;
    private int charsAtMaxFontSize = 2;
    private int charsAtMinFontSize = 8;
    private float? fontSizeOverride;
    private bool hasBackgroundPrefab;

    // 呼び出しのたびにTextMeshProを子として生成する。背景ImageはbackgroundPrefabがあればそれをインスタンス化し、
    // 無ければ従来通りcardObject自体にImageをAddComponentする(既存呼び出し元との後方互換)。
    // minFontSize <= 0 の場合は文字数による自動縮小を行わない(baseFontSize固定)。
    public static WordCardView Create(
        Transform parent,
        float fontSize = 24f,
        Color? fontColor = null,
        TMP_FontAsset fontAsset = null,
        float minFontSize = 0f,
        int charsAtMaxFontSize = 2,
        int charsAtMinFontSize = 8,
        GameObject backgroundPrefab = null)
    {
        var cardObject = new GameObject("WordCard", typeof(RectTransform));
        var cardTransform = (RectTransform)cardObject.transform;
        cardTransform.SetParent(parent, false);
        cardTransform.pivot = new Vector2(0.5f, 0.5f);
        cardTransform.anchorMin = new Vector2(0.5f, 0.5f);
        cardTransform.anchorMax = new Vector2(0.5f, 0.5f);

        Image background;
        if (backgroundPrefab != null)
        {
            var backgroundInstance = Object.Instantiate(backgroundPrefab, cardTransform, false);
            var backgroundTransform = (RectTransform)backgroundInstance.transform;
            backgroundTransform.anchorMin = Vector2.zero;
            backgroundTransform.anchorMax = Vector2.one;
            backgroundTransform.offsetMin = Vector2.zero;
            backgroundTransform.offsetMax = Vector2.zero;

            background = backgroundInstance.GetComponent<Image>();
            if (background == null)
                Debug.LogWarning("WordCardView: backgroundPrefab に Image コンポーネントがありません");
        }
        else
        {
            background = cardObject.AddComponent<Image>();
            background.enabled = false;
        }

        var labelObject = new GameObject("Label", typeof(RectTransform));
        var labelTransform = (RectTransform)labelObject.transform;
        labelTransform.SetParent(cardTransform, false);
        labelTransform.anchorMin = Vector2.zero;
        labelTransform.anchorMax = Vector2.one;
        labelTransform.offsetMin = Vector2.zero;
        labelTransform.offsetMax = Vector2.zero;

        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.color = fontColor ?? Color.black;
        label.fontSize = fontSize;
        if (fontAsset != null) label.font = fontAsset;

        var view = cardObject.AddComponent<WordCardView>();
        view.background = background;
        view.hasBackgroundPrefab = backgroundPrefab != null;
        view.label = label;
        view.baseFontSize = fontSize;
        view.minFontSize = minFontSize > 0f ? minFontSize : fontSize;
        view.charsAtMaxFontSize = Mathf.Max(0, charsAtMaxFontSize);
        view.charsAtMinFontSize = Mathf.Max(view.charsAtMaxFontSize + 1, charsAtMinFontSize);
        return view;
    }

    // 手動でフォントサイズを固定する(以降SetWord/SetFixedTextを呼んでも自動縮小より優先される)。
    public void SetFontSizeOverride(float size)
    {
        fontSizeOverride = size;
        label.fontSize = size;
    }

    // 単語のはめ込みは文字だけ更新する。背景画像はSetEmpty/SetFixedText時のまま維持する。
    public void SetWord(WordAsset word)
    {
        label.text = word.DisplayText;
        ApplyAutoFontSize(word.DisplayText?.Length ?? 0);
    }

    public void SetFixedText(string text, PlayerTextSpriteData spriteData = null)
    {
        label.text = text;
        ApplyAutoFontSize(text?.Length ?? 0);
        ApplySprite(spriteData != null ? spriteData.sprite : null);
    }

    public void SetEmpty(PlayerTextSpriteData spriteData = null)
    {
        label.text = "";
        ApplySprite(spriteData != null ? spriteData.sprite : null);
    }

    private void ApplyAutoFontSize(int charCount)
    {
        if (fontSizeOverride.HasValue)
        {
            label.fontSize = fontSizeOverride.Value;
            return;
        }

        label.fontSize = ComputeFontSize(charCount, baseFontSize, minFontSize, charsAtMaxFontSize, charsAtMinFontSize);
    }

    // charsAtMaxFontSize文字以下ならbaseFontSize、charsAtMinFontSize文字以上ならminFontSize、間は線形補間。
    // SentenceLayoutEditorのプレビュー表示でも同じ計算式を使う。
    public static float ComputeFontSize(
        int charCount,
        float baseFontSize,
        float minFontSize,
        int charsAtMaxFontSize,
        int charsAtMinFontSize)
    {
        if (charCount <= charsAtMaxFontSize) return baseFontSize;
        if (charCount >= charsAtMinFontSize) return minFontSize;

        float t = (float)(charCount - charsAtMaxFontSize) / (charsAtMinFontSize - charsAtMaxFontSize);
        return Mathf.Lerp(baseFontSize, minFontSize, t);
    }

    // 枠Prefab使用時は、カード個別のスプライト上書きが無い限りenabled/spriteを触らない
    // (常時ループのアニメーション付き枠を消してしまわないため)。
    private void ApplySprite(Sprite sprite)
    {
        if (background == null) return;

        if (sprite != null)
        {
            background.sprite = sprite;
            background.enabled = true;
            return;
        }

        if (!hasBackgroundPrefab)
        {
            background.enabled = false;
        }
    }
}