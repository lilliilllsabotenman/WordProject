using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MouseControllUI : MouseControllBase
{
    [Header("追従先Canvas")]
    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        canvasRectTransform = (RectTransform)canvas.transform;
    }

    protected override void UpdateMousePosition()
    {
        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            Input.mousePosition,
            eventCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }
}
