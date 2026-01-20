using System.Collections;
using System.Collections.Generic;
using RootMotion;
using UnityEngine;

public class TimeSystem : Singleton<TimeSystem>
{
    private int totalGameTicks;
    public float timeFloat;
    public int Hour;
    public int GetCurrentTick => (int)(timeFloat* 24f * 60f);

    private void InitializeTimeSystem()
    {
        timeFloat = (float)Hour / 24f / 1440f;
    }


    private void Tick (int tick)
    {
        totalGameTicks++;
    }
}