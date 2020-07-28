using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(ProjectileController))]
public class ExplosiveController : NetworkBehaviour
{
    private ProjectileController projectileController;

    [SerializeField]
    private GameObject impact = null;

    [Header("Explosive Operative")]

    [SerializeField]
    private float fuse;

    [SerializeField]
    private bool explodeOnImpact = true;

    [SerializeField]
    private bool airburst = false;

    [SerializeField]
    private float force = 10f;

    [SerializeField]
    private LayerMask mask = -1;

    [Header("Damage")]

    [SerializeField]
    private int damage = 100;

    [SerializeField]
    private float range = 10f;

    [SerializeField]
    private DamageType damageType;

    [SerializeField]
    private AnimationCurve damageFallOff = null;

    [SerializeField]
    private AnimationCurve damageOverTime = null;

    [SerializeField]
    private AnimationCurve damageOverAngle = null;

    private const string PLAYER_TAG = "Player";

    [HideInInspector]
    public float timeSinceCreated = 0f;

    private bool impacted = false;

    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        timeSinceCreated += Time.deltaTime;
        if (fuse <= timeSinceCreated && GetComponent<NetworkIdentity>().hasAuthority)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        Vector3 _dir = projectileController.rb.velocity;

        if (_dir.magnitude < 1f)
            _dir = Vector3.up;

        CmdExplode(_dir, timeSinceCreated);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (explodeOnImpact && !impacted && GetComponent<NetworkIdentity>().hasAuthority)
        {
            impacted = true;
            projectileController.rb.isKinematic = true;
            CmdExplode(collision.contacts[0].normal * (1 - 2*Convert.ToSingle(airburst)), timeSinceCreated);
        }
    }

    [Command]
    void CmdExplode(Vector3 _dir, float _timeSinceCreated)
    {
        RpcExplode(Quaternion.LookRotation(_dir));

        Collider[] colliders = Physics.OverlapSphere(transform.position, range);

        foreach (var _collider in colliders)
        {

            if (_collider.tag == PLAYER_TAG && _collider.name == "Head")
            {

                RaycastHit _hit;

                Vector3 target_vector = _collider.transform.position - transform.position;

                if (Physics.Raycast(transform.position, target_vector, out _hit, range, mask))
                {
                    if (_collider.tag == PLAYER_TAG)
                    {
                        float _distance = Vector3.Distance(_hit.transform.position, transform.position);

                        Player player = _hit.transform.root.GetComponent<Player>();

                        if (player != null)
                        {
                            player.RpcTakeDamage((int)(damage * damageFallOff.Evaluate(_distance / range) * damageOverTime.Evaluate(_timeSinceCreated) * damageOverAngle.Evaluate(Vector3.Angle(_dir, target_vector)/180f)), projectileController.playerID, damageType.name);
                        }
                    }
                }
            }

        }
        
    }

    [ClientRpc]
    public void RpcExplode(Quaternion _rot)
    {
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);

        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        foreach (Collider _collider in colliders)
        {
            Rigidbody rb = _collider.attachedRigidbody;

            if (rb != null)
                rb.AddExplosionForce(force, transform.position, range, 1.0f);
        }

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }

}


