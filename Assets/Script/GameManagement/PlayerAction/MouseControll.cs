using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class aMouseControll : MonoBehaviour
{
    [Header("Input Filter")]
    public LayerMask wordLayer;

    [Header("Game Management")]
    [SerializeField] private GameManagement gameManagtement;

    [Header("Sentence Text UI")]
    [SerializeField] private SentenceText sentenceText;

    private void Update()
    {
        if (gameManagtement == null)
        {
            gameManagtement = Object.FindFirstObjectByType<GameManagement>();
        }

        if (Camera.main == null)
        {
            return;
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition;

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (!GetClickedUIObject(
                Input.mousePosition,
                out GameObject obj,
                filter: go => go.GetComponentInParent<WordObject>() != null))
        {
            return;
        }

        var wordObject = obj.GetComponentInParent<WordObject>();
        if (wordObject == null)
        {
            return;
        }

        var sentenceState = gameManagtement != null
            ? gameManagtement.GetOrCreateSentenceState()
            : null;

        if (sentenceState == null)
        {
            Debug.LogWarning("MouseControll: sentenceState is null.");
            return;
        }

        if (wordObject.TryGetWordAsset(out var asset))
        {
            sentenceState.TryAddWord(asset, out _);
        }
    }

    public bool GetClickedUIObject(
        Vector2 mouseScreenPos,
        out GameObject obj,
        IEnumerable<Canvas> canvases = null,
        System.Predicate<GameObject> filter = null)
    {
        obj = null;

        if (EventSystem.current == null)
        {
            return false;
        }

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = mouseScreenPos
        };

        var targetCanvases = canvases ?? Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var results = new List<RaycastResult>();

        foreach (var c in targetCanvases)
        {
            var raycaster = c.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                continue;
            }

            raycaster.Raycast(pointer, results);
        }

        if (results.Count == 0)
        {
            return false;
        }

        var sorted = results
            .OrderByDescending(r => r.depth)
            .ThenByDescending(r => r.sortingOrder)
            .ThenBy(r => r.distance);

        foreach (var r in sorted)
        {
            var go = r.gameObject;

            if (filter == null || filter(go))
            {
                obj = go;
                return true;
            }
        }

        return false;
    }
}
