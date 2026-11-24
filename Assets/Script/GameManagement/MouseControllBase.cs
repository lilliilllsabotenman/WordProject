using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public abstract class MouseControllBase : MonoBehaviour
{
    [Header("Input Filter")]
    public LayerMask wordLayer;

    public bool IsControll = true;//API化必須
    private event Action<WordAsset> PushWordAsset;

    private void Update()
    {
        UpdateMousePosition();

        if(!IsControll) return;

        if (Input.GetMouseButtonDown(0))
        {
            if(TryGetHoveredWordAsset(out WordAsset asset))
            {
                PushWordAsset?.Invoke(asset);
            }
        }

    // Debug.Log(IsControll);
    }

    protected abstract void UpdateMousePosition();

    public bool TryGetHoveredWordAsset(out WordAsset asset)
    {
        asset = null;

        foreach(Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            WordObject[] words = canvas.GetComponentsInChildren<WordObject>(true);

            foreach(WordObject word in words)
            {
                if(!word.gameObject.activeInHierarchy)
                    continue;

                RectTransform rect = word.GetComponent<RectTransform>();

                if(rect == null)
                    continue;

                if(RectTransformUtility.RectangleContainsScreenPoint(
                    rect,
                    Input.mousePosition))
                {
                    return word.TryGetWordAsset(out asset);
                }
            }
        }

        return false;
    }

    public void MouseSubscrive(Action<WordAsset> action)
    {
        PushWordAsset += action;
    }

}
