using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplosiveController : NetworkBehaviour
{
    private ProjectileController projectileController;

    [SerializeField]
    private GameObject impact;

    [SerializeField]
    private int damage;

    [SerializeField]
    private float range;

    [SerializeField]
    private float fuse;

    [SerializeField]
    private AnimationCurve damageFallOff;

    [SerializeField]
    private float force;

    [SerializeField]
    private LayerMask mask;

    private const string PLAYER_TAG = "Player";


    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        fuse -= Time.deltaTime;
        if (fuse <= 0)
        {
            CmdExplode(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        projectileController.rb.isKinematic = true;
        CmdExplode(Quaternion.LookRotation(collision.contacts[0].normal));
    }

    [Command]
    void CmdExplode(Quaternion _rot)
    {
        RpcExplode(_rot);

        Collider[] colliders = Physics.OverlapSphere(transform.position, range);

        foreach (var _collider in colliders)
        {

            if (_collider.tag == PLAYER_TAG)
            {

                RaycastHit _hit;
                if (Physics.Raycast(transform.position, _collider.transform.position - transform.position, out _hit, range, mask))
                {
                    if (_collider.tag == PLAYER_TAG)
                    {
                        float _distance = Vector3.Distance(_hit.transform.position, transform.position);

                        Player player = _hit.transform.root.GetComponent<Player>();

                        if (player != null)
                            player.RpcTakeDamage((int)(damageFallOff.Evaluate(_distance / range) * damage), projectileController.playerID);
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


