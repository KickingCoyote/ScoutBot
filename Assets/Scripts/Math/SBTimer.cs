using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SBTimer
{
    private float startTime;

    public void StartTimer()
    {
        startTime = Time.realtimeSinceStartup;
    }

    public float Timer()
    {
        return Time.realtimeSinceStartup - startTime;
    }

}
