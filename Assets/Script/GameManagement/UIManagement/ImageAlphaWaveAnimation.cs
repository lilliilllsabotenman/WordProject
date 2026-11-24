using UnityEngine;
using UnityEngine.UI;

public class ImageAlphaWaveAnimation : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;

    private Image image;

    private void Awake()
    {
        image = this.GetComponent<Image>();
        if(image == null)
        {
            this.enabled = false;
            return;
        }
    }

    private void Update()
    {
        float alpha = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
