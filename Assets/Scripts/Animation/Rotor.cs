using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotor : MonoBehaviour
{
    [SerializeField]
    private float speed = 1000f;

    void Update()
    {
        transform.Rotate(0.0f, speed * Time.deltaTime, 0.0f, Space.Self);
    }
}
