using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAngularVelocity : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private Vector3 angularVelocity = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = transform.TransformVector(angularVelocity);
    }
}
