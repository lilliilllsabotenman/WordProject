using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class MouseControll : MonoBehaviour
{
    [Header("Input Filter")]
    public LayerMask wordLayer;

    private event Action<WordAsset> PushWordAsset;

    private void Update()
    {
        UpdateMousePosition();

        if (Input.GetMouseButtonDown(0))
        {
            if(TryGetHoveredWordAsset(out WordAsset asset))
            {
                PushWordAsset?.Invoke(asset);
            }
        }
    }

    private void UpdateMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition;
    }

    public bool TryGetHoveredWordAsset(out WordAsset asset)
    {
        asset = null;

        foreach(Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            WordObject[] words = canvas.GetComponentsInChildren<WordObject>(true);

            foreach(WordObject word in words)
            {
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
