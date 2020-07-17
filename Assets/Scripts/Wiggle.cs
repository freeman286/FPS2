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

    private float time;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb.isKinematic)
        {
            time = 0;
        }
        else
        {
            float wiggle = wiggleAmplitude.Evaluate(time) * maxWiggleAmplitude;
            rb.velocity += transform.right * Mathf.Sin(time * wiggleRate) * wiggle + transform.up * Mathf.Cos(time * wiggleRate) * wiggle;
            time += Time.deltaTime;
        }
    }
}
