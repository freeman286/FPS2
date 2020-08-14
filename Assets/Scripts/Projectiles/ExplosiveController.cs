﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplosiveController : ProjectileController
{

    [Header("Explosive Operative")]

    [SerializeField]
    private float fuse;

    [SerializeField]
    private bool explodeOnImpact = true;

    private bool impacted = false;

    private Explosive explosive;

    public override void Start()
    {
        base.Start();
        explosive = GetComponent<Explosive>();
    }

    public override void Update()
    {
        base.Update();
        if (fuse <= timeSinceCreated && GetComponent<NetworkIdentity>().hasAuthority)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        Vector3 _dir = rb.velocity;

        if (_dir.magnitude < 1f)
            _dir = Vector3.up;

        explosive.CmdExplode(transform.position, _dir, timeSinceCreated, playerID);
    }

    public void ForceDetonate(string _sourceID)
    {
        playerID = _sourceID;

        Detonate();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (explodeOnImpact && !impacted && GetComponent<NetworkIdentity>().hasAuthority)
        {
            impacted = true;
            rb.isKinematic = true;
            explosive.CmdExplode(transform.position, collision.contacts[0].normal, timeSinceCreated, playerID);
        }
    }

}


