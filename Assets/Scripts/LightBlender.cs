using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightBlender : MonoBehaviour
{
    public float duration = 1.0f;
    private Light lt;

    void Start()
    {
        lt = GetComponent<Light>();
    }

    void Update()
    {
        float phi = Time.time / duration * 2 * Mathf.PI;
        float amplitude = Mathf.Cos(phi) * 0.5f + 0.5f;
        lt.intensity = amplitude;
    }
}
    