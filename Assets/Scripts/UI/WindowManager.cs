using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    private const string RESOLUTION_PREF_KEY = "resolution";

    [SerializeField]
    private Text resolutionText;

    Resolution[] resolutions;

    private int currentResolutionIndex = 0;

    private int screenResolutionIndex = 0;

    void Start()
    {
        resolutions = Screen.resolutions;

        currentResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_PREF_KEY, resolutions.Length - 1);
        ApplyResolution(resolutions[currentResolutionIndex]);

        Pause.instance.pausedCallback += ResetResolutionText;
    }

    public void ResetResolutionText()
    {
        SetResolutionText(resolutions[screenResolutionIndex]);
    }

    private void SetResolutionText(Resolution resolution)
    {
        resolutionText.text = resolution.width + "x" + resolution.height;
    }

    public void SetNextResolution()
    {
        currentResolutionIndex = Util.GetNextWrappedIndex(resolutions, currentResolutionIndex);
        SetResolutionText(resolutions[currentResolutionIndex]);
    }

    public void SetPreviousResolution()
    {
        currentResolutionIndex = Util.GetPreviousWrappedIndex(resolutions, currentResolutionIndex);
        SetResolutionText(resolutions[currentResolutionIndex]);
    }

    public void ApplyCurrentResolution()
    {
        ApplyResolution(resolutions[currentResolutionIndex]);
    }

    private void ApplyResolution(Resolution _resolution)
    {
        SetResolutionText(_resolution);

        Screen.SetResolution(_resolution.width, _resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(RESOLUTION_PREF_KEY, currentResolutionIndex);
        screenResolutionIndex = currentResolutionIndex;
    }
}