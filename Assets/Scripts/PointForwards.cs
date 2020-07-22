using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(ProjectileController))]
public class PointForwards : NetworkBehaviour
{
    private Rigidbody rb;
    private NetworkIdentity networkIdentity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    void Update()
    {
        if (rb != null && !rb.isKinematic && networkIdentity.hasAuthority)
            transform.rotation = Quaternion.LookRotation(rb.velocity);
    }
}
