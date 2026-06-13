using System;
using System.Collections.Generic;
using System.Linq;

public class WordSlotState
{
    private readonly List<WordAsset> currentAnswers = new();

    public IReadOnlyList<WordAsset> CurrentAnswers => currentAnswers;

    public event Action OnChanged;
    public event Action<WordAsset, int> OnWordPlaced;
    public event Action<int> OnSlotCleared;

    public WordSlotState(int slotCount)
    {
        InitializeSlots(slotCount);
    }

    public bool TryAddWord(WordAsset asset, out int slotIndex, bool allowOverwriteWhenFull = false)
    {
        slotIndex = -1;

        if (asset == null || currentAnswers.Count == 0)
        {
            return false;
        }

        slotIndex = currentAnswers.FindIndex(x => x == null);

        if (slotIndex < 0)
        {
            if (!allowOverwriteWhenFull)
            {
                return false;
            }

            slotIndex = currentAnswers.Count - 1;
        }

        currentAnswers[slotIndex] = asset;

        OnWordPlaced?.Invoke(asset, slotIndex);
        NotifyChanged();

        return true;
    }


    public void ClearAll()
    {
        for (int i = 0; i < currentAnswers.Count; i++)
        {
            currentAnswers[i] = null;
            OnSlotCleared?.Invoke(i);
        }

        NotifyChanged();
    }

    public bool IsFull()
    {
        return currentAnswers.All(x => x != null);
    }

    public void NotifyChanged()
    {
        OnChanged?.Invoke();
    }

    private void InitializeSlots(int slotCount)
    {
        currentAnswers.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            currentAnswers.Add(null);
        }
    }
}
