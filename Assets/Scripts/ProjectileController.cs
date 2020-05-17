using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class ProjectileController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    public Rigidbody rb;

    public float initialVelocity;

    public float constantForce;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * initialVelocity;
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * constantForce);
    }
    
}
