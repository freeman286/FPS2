using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAngularVelocity : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private float tumble = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = Random.insideUnitSphere * tumble;
    }

}
