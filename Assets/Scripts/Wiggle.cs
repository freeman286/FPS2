using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    [HideInInspector]
    private Rigidbody rb;

    [SerializeField]
    private float wiggleRate;

    [SerializeField]
    private AnimationCurve wiggleAmplitude;

    [SerializeField]
    private float maxWiggleAmplitude;

    private float initializationTime;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initializationTime = Time.timeSinceLevelLoad;
    }

    void Update()
    {
        if (rb.isKinematic)
            initializationTime = Time.timeSinceLevelLoad;

        float time = Time.timeSinceLevelLoad - initializationTime;
        float wiggle = wiggleAmplitude.Evaluate(time) * maxWiggleAmplitude;
        rb.velocity += transform.right * Mathf.Sin(time * wiggleRate) * wiggle + transform.up * Mathf.Cos(time * wiggleRate) * wiggle;
    }
}
