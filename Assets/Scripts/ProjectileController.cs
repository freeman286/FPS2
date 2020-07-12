using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class ProjectileController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    [HideInInspector]
    public Rigidbody rb;

    public float constantForce;

    public Collider[] colliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Command]
    public void CmdLaunch(float _velocity)
    {
        RpcLaunch(_velocity);
    }

    [ClientRpc]
    public void RpcLaunch(float _velocity)
    {
        rb = GetComponent<Rigidbody>(); // Make sure rb is defined if we call this as soon as the projectile is spawned
        rb.velocity = transform.forward * _velocity;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.AddForce(transform.forward * constantForce);
        }
    }
    
}
