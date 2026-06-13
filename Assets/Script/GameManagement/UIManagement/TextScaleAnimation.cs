using UnityEngine;
using System;

public class TextScaleAnimation : MonoBehaviour
{
    [SerializeField]
    private float amplitude = 0.2f;

    [SerializeField]
    private float speed = 3f;

    private Vector3 baseScale;
    private TMPro.TextMeshProUGUI TMP;

    private float elapsedTime = 0f;
    private float limitTime = 0f;
    private bool isRunning = false;

    private event Action IsTimeUp;

    private void Awake()
    {
        TMP = this.GetComponent<TMPro.TextMeshProUGUI>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if(!isRunning) return;

        elapsedTime += Time.deltaTime;

        float scale = 1f + (elapsedTime * speed) * amplitude;
        transform.localScale = baseScale * scale;

        if(elapsedTime >= limitTime) 
        {
            IsTimeUp?.Invoke();
        }
    }

    public void isFinished()
    {
        this.gameObject.SetActive(false);
    }

    public void InitializeEnemyText(EnemyData data, Action action)
    {
        if(TMP == null) return;
        transform.localScale = baseScale;
        TMP.text = data.enemyText;
        elapsedTime = 0;
        limitTime = data.limitTime;
        isRunning = true;

        IsTimeUp += action;
    }
}