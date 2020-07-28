using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StunController : NetworkBehaviour
{
    private ProjectileController projectileController;

    [SerializeField]
    private GameObject impact = null;

    public float fuse;

    [SerializeField]
    private LayerMask mask = -1;

    [Header("Stun Operation")]

    [SerializeField]
    private float amount = 5f;

    [SerializeField]
    private float range = 10f;

    private const string PLAYER_TAG = "Player";

    private float timeSinceCreated;

    private bool impacted = false;

    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        timeSinceCreated += Time.deltaTime;
        fuse -= Time.deltaTime;
        if (fuse <= 0 && GetComponent<NetworkIdentity>().hasAuthority)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        Vector3 _dir = projectileController.rb.velocity;

        if (_dir.magnitude < 1f)
            _dir = Vector3.up;

        CmdExplode(_dir);
    }

    [Command]
    void CmdExplode(Vector3 _dir)
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

                        PlayerStun _playerStun = _hit.transform.root.GetComponent<PlayerStun>();

                        if (_playerStun != null)
                        {
                            _playerStun.RpcStun(amount * (range - _distance) / range);
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

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }
}
