using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class Environment : MonoBehaviour
{
    public static Environment instance;

    private Volume volume;

    private VolumeProfile volumeProfile;

    private LensDistortion lensDistortion;

    private ChromaticAberration chromaticAberration;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one Environment class in scene.");
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        volume = GetComponent<Volume>();
        volumeProfile = volume.profile;

        LensDistortion tmpLD;
        if (volumeProfile.TryGet<LensDistortion>(out tmpLD))
        {
            lensDistortion = tmpLD;
        }

        ChromaticAberration tmpCA;
        if (volumeProfile.TryGet<ChromaticAberration>(out tmpCA))
        {
            chromaticAberration = tmpCA;
        }

        UnScope();
    }

    public void Scope()
    {
        lensDistortion.intensity.value = 0.6f;
        chromaticAberration.intensity.value = 0.6f;
    }

    public void UnScope()
    {
        lensDistortion.intensity.value = 0;
        chromaticAberration.intensity.value = 0;
    }

}
