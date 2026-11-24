using UnityEngine;

public class MouseControll : MouseControllBase
{
    protected override void UpdateMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition;
    }
}
