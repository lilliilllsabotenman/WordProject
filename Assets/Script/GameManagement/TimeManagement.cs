using System;

public class TimeManagement
{
    private float elapsedTime = 0f;
    private float limitTime = 0f;
    private bool isRunning = false;

    // ラウンド開始時に呼ぶ
    public void Initialize(float limitTime)
    {
        this.limitTime = Math.Max(0f, limitTime);
        elapsedTime = 0f;
        isRunning = true;
    }

    // true を返したフレームで時間切れ
    public bool OnUpdate(float deltaTime)
    {
        if (!isRunning)
        {
            return false;
        }

        elapsedTime += deltaTime;

        if (elapsedTime < limitTime)
        {
            return false;
        }

        isRunning = false;
        return true;
    }
}
