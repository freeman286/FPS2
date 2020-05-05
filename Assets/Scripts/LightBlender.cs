﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightBlender : MonoBehaviour
{
    [SerializeField]
    private float duration = 1.0f;

    private Light lt;

    [SerializeField]
    private float maxLightLevel;

    [SerializeField]
    private AnimationCurve lightLevel;

    private float time;

    void Start()
    {
        lt = GetComponent<Light>();
    }

    void Update()
    {
        lt.intensity = lightLevel.Evaluate(time / duration) * maxLightLevel;
        time += Time.deltaTime; 
    }
}
    