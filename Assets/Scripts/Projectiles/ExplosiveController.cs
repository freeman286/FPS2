using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplosiveController : ProjectileController
{
    [SerializeField]
    private GameObject impact = null;

    [Header("Explosive Operative")]

    [SerializeField]
    private float fuse;

    [SerializeField]
    private bool explodeOnImpact = true;

    [SerializeField]
    private bool airburst = false;

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

    public void ForceDetonate()
    {
        Health _health = GetComponent<Health>();

        if (_health != null)
            playerID = _health.lastDamagedPlayer.transform.name;

        Detonate();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (explodeOnImpact && !impacted && GetComponent<NetworkIdentity>().hasAuthority)
        {
            impacted = true;
            rb.isKinematic = true;
            explosive.CmdExplode(transform.position, collision.contacts[0].normal * (1 - 2*Convert.ToSingle(airburst)), timeSinceCreated, playerID);
        }
    }

}


