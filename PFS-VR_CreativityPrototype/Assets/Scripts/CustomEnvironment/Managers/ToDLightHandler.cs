using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapted from this tutorial: https://www.youtube.com/watch?v=33RL196x4LI
/// </summary>
public class ToDLightHandler : MonoBehaviour
{
    public static ToDLightHandler instance;

    private bool _daylightEnabled = true;
    private bool sunWasEnabled = true;
    private bool daylightEnablingCalled = false; //flag to detect first setter call
    public bool DaylightEnabled
    {
        get { return _daylightEnabled; }
        set
        {
            daylightEnablingCalled = true;
            if (value)
            {
                //reactivate
                if (sunWasEnabled) sun.gameObject.SetActive(true);
                else moon.gameObject.SetActive(true);
            } else
            {
                //switch off
                sunWasEnabled = sun.gameObject.activeSelf;
                sun.gameObject.SetActive(false);
                moon.gameObject.SetActive(false);
            }
            _daylightEnabled = value;
        }
    }

    public Vector3 noonSunEulerAngle;

    [Header("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Environment Lighting")]
    public AnimationCurve skyBoxLightingIntensity;
    public AnimationCurve skyBoxReflectionsIntensity;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        if (!daylightEnablingCalled)
        {
            DaylightEnabled = true;
        }
    }

    public void updateToD(float newToD)
    {
        sun.transform.eulerAngles = (newToD - 0.25f) * noonSunEulerAngle * 4.0f;
        moon.transform.eulerAngles = (newToD - 0.75f) * noonSunEulerAngle * 4.0f;

        sun.intensity = sunIntensity.Evaluate(newToD);
        moon.intensity = moonIntensity.Evaluate(newToD);

        sun.color = sunColor.Evaluate(newToD);
        moon.color = moonColor.Evaluate(newToD);

        swapMoonAndSun();

        RenderSettings.ambientIntensity = skyBoxLightingIntensity.Evaluate(newToD);
        RenderSettings.reflectionIntensity = skyBoxReflectionsIntensity.Evaluate(newToD);
    }

    private void swapMoonAndSun()
    {
        if (sun.intensity == 0 && sun.gameObject.activeInHierarchy)
        {
            sun.gameObject.SetActive(false);
        }
        else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
        {
            sun.gameObject.SetActive(true);
        }

        if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
        {
            moon.gameObject.SetActive(false);
        }
        else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
        {
            moon.gameObject.SetActive(true);
        }
    }

}
