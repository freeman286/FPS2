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

    public float initialVelocity;

    public float constantForce;

    public Collider[] colliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Launch();
    }

    public void Launch()
    {
        CmdLaunch();
    }

    [Command]
    void CmdLaunch()
    {
        RpcLaunch();
    }

    [ClientRpc]
    void RpcLaunch()
    {
        rb.velocity = transform.forward * initialVelocity;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.AddForce(transform.forward * constantForce);
        }
    }
    
}
