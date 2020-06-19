using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionCorrector : MonoBehaviour
{

    [SerializeField]
    private int manualScreenWidth = 1920;

    void Start()
    {
        
        Screen.SetResolution(manualScreenWidth, Mathf.RoundToInt(1f * manualScreenWidth * Screen.currentResolution.height / Screen.currentResolution.width), Screen.fullScreen);

    }

}
