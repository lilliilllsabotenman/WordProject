using UnityEngine;
using System;

[Serializable]
public class LifeManager
{
    [SerializeField] private GameObject[] objects;

    private int currentLife;
    private Action Finish;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        currentLife = objects.Length;
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
    }
}