using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength;
    public float startTime = 0.4f;
    private float timeRate;
    public Vector3 noon;
    public int currentHour;
    public int currentMinute;
    public TextMeshProUGUI timeText;

    [Header ("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplier;
    public AnimationCurve reflectionsIntensityMultiplier;

    void Start ()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
    }

    void Update ()
    {
        // increment time
        time += timeRate * Time.deltaTime;

        if(time >= 1.0f)
            time = 0.0f;

        // calculate current hour and minute
        currentHour = Mathf.FloorToInt(24.0f * time);
        currentMinute = Mathf.FloorToInt(60.0f * (24.0f * time - currentHour));

        timeText.SetText(string.Format("{0:00}:{1:00}", currentHour, currentMinute));

        // light rotation
        sun.transform.eulerAngles = (time - 0.25f) * noon * 4.0f;
        moon.transform.eulerAngles = (time - 0.75f) * noon * 4.0f;

        // light intensity
        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(time);

        // change colors
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(time);

        // enable / disable the sun
        if(sun.intensity == 0 && sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(false);
        else if(sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(true);

        if(moon.intensity == 0 && moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(false);
        else if(moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(true);

        // lighting and reflections intensity
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(time);
        


    }

    public void SetTime(float newTime)
    {
        time = newTime;
    }

    public void SetFullDayLength(float newLength)
    {
        fullDayLength = newLength;
        timeRate = 1.0f / fullDayLength;
    }

    public void SetStartTime(float newTime)
    {
        startTime = newTime;
        time = startTime;
    }



}
