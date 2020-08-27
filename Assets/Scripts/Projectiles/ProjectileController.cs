﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

public class ProjectileController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    [HideInInspector]
    public Rigidbody rb;

    public new float constantForce;

    [Header("Physical Properties")]

    public Collider[] colliders;

    public VisualEffect particles;
    public Light lightEffect;

    [HideInInspector]
    public float timeSinceCreated = 0f;

    protected const string PLAYER_TAG = "Player";

    protected NetworkIdentity networkIdentity;

    private bool active = true;

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    public virtual void Update()
    {
        if (active)
        {
            timeSinceCreated += Time.deltaTime;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.RegisterProjectile(GetComponent<NetworkIdentity>().netId.ToString(), gameObject);
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
        if (networkIdentity.hasAuthority && rb != null)
        {
            rb.AddForce(transform.forward * constantForce);
        }
    }

    public void Activate(string _playerID, bool _active)
    {
        active = _active;
        timeSinceCreated = 0f;

        playerID = _playerID;

        Health _health = GetComponent<Health>();

        if (_health != null)
            _health.playerID = _playerID;

        if (particles != null)
            particles.enabled = _active;

        if (lightEffect != null)
            lightEffect.enabled = _active;
    }

    void OnDestroy()
    {
        GameManager.UnRegisterProjectile(transform.name);
    }
}
