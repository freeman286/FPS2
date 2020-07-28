using System.Collections;
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

    private float fuse;

    private ExplosiveController explosiveController;
    private ImpactController impactController;
    private StunController stunController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        explosiveController = GetComponent<ExplosiveController>();
        impactController = GetComponent<ImpactController>();
        stunController = GetComponent<StunController>();

        if (explosiveController != null)
        {
            fuse = explosiveController.fuse;
        }
        else if (impactController != null)
        {
            fuse = impactController.fuse;
        }
        else if (stunController != null)
        {
            fuse = stunController.fuse;
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
        if (rb != null)
        {
            rb.AddForce(transform.forward * constantForce);
        }
    }

    public void Activate(string _playerID, bool _active)
    {
        if (explosiveController != null)
        {
            explosiveController.fuse = fuse;
            explosiveController.enabled = _active;
        }
        else if (impactController != null)
        {
            impactController.fuse = fuse;
            impactController.enabled = _active;
        } else if (stunController != null)
        {
            stunController.fuse = fuse;
            stunController.enabled = _active;
        }

        playerID = _playerID;

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
