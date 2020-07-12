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

    [SyncVar]
    private string name;

    void Start()
    {

        bool _named = false;
        int _index = 0;

        while (!_named) {

            if (playerID != null && GameObject.Find(playerID + "_Projectile_" + _index) == null)
            {
                name = playerID + "_Projectile_" + _index;
                _named = true;
            }

            if (playerID != null)
                _index++;
        }

        transform.name = name;

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
