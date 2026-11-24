using UnityEngine;
using TMPro;
using System;

[Serializable]
public class LifeManager
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private TextMeshProUGUI lifeText;

    private int currentLife;
    private Action Finish;

    public void Initialize()
    {
        int ActiveObjectCount = 0;
        foreach(var obj in objects)
        {
            if(obj.activeSelf) ActiveObjectCount ++;
        }
        currentLife = ActiveObjectCount;
        UpdateLifeDisplay();
    }

    public void Subscribe(Action action)
    {
        Finish += action;
    }

    public void DecreaseLife()
    {
        if (currentLife <= 0)
            return;

        currentLife--;

        UpdateLifeDisplay();

        if (currentLife <= 0)
            Finish?.Invoke();
    }

    private void UpdateLifeDisplay()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;

            objects[i].SetActive(i < currentLife);
        }

        if (lifeText != null)
        {
            lifeText.text = currentLife.ToString();
        }
    }
}
