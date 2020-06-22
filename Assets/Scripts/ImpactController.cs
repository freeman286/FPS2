using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ImpactController : NetworkBehaviour
{
    private ProjectileController projectileController;

    [SerializeField]
    private GameObject impact;

    [SerializeField]
    private int damage;

    [SerializeField]
    private float fuse;

    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        fuse -= Time.deltaTime;
        if (fuse <= 0)
        {
            CmdImpact(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up), null);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        projectileController.rb.isKinematic = true;

        Player _player = collision.transform.root.GetComponent<Player>();

        string _playerID = null;
        if (_player != null)
        {
            _playerID = _player.transform.name;
        }

        CmdImpact(Quaternion.LookRotation(collision.contacts[0].normal), _playerID);
    }

    [Command]
    void CmdImpact(Quaternion _rot, string _playerID)
    {
        RpcImpact(_rot);

        if (_playerID != null)
        {
            Player _player = GameManager.GetPlayer(_playerID);
            _player.RpcTakeDamage(damage, projectileController.playerID);
        }
    }

    [ClientRpc]
    public void RpcImpact(Quaternion _rot)
    {
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);

        Destroy(_impact, 10f);
        NetworkServer.Destroy(gameObject);
    }

}

