using UnityEngine;

public class TextScaleAnimation : MonoBehaviour
{
    [SerializeField]
    private float amplitude = 0.2f;

    [SerializeField]
    private float speed = 3f;

    private Vector3 baseScale;

    private TMPro.TextMeshProUGUI TMP;

    private float time = 0;

    private void Awake()
    {
        TMP = this.GetComponent<TMPro.TextMeshProUGUI>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        time += Time.deltaTime;

        float scale = 1f + (time * speed) * amplitude;
        transform.localScale = baseScale * scale;
    }

    public void isFinished()
    {
        this.gameObject.SetActive(false);
    }

    public void SetText(string txt)
    {
        if(TMP == null) return;
        transform.localScale = baseScale;
        TMP.text = txt;
    }
}