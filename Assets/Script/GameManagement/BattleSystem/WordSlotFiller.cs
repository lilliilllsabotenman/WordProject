using System.Collections.Generic;

/// <summary>
/// はめ込み役：配置役から受け取ったスロットカード参照に、出現順でWordAssetをはめ込む。
/// </summary>
public class WordSlotFiller
{
    private List<WordCardView> slotCards = new();
    private int nextIndex;

    public void Initialize(List<WordCardView> slotCards)
    {
        this.slotCards = slotCards;
        nextIndex = 0;
    }

    public void AddWord(WordAsset word)
    {
        if (nextIndex >= slotCards.Count) return;

        slotCards[nextIndex].SetWord(word);
        nextIndex++;
    }
}
