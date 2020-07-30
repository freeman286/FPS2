using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ImpactController : ProjectileController
{
    [SerializeField]
    private GameObject impact = null;

    [Header("Impact Operative")]

    [SerializeField]
    private float fuse = 10f;

    [SerializeField]
    private bool sticky = false;

    [Header("Damage")]

    [SerializeField]
    private int damage = 100;

    [SerializeField]
    private DamageType damageType;

    [SerializeField]
    private float headShotMultiplier = 2f;

    public override void Update()
    {
        base.Update();
        if (fuse <= timeSinceCreated && transform.parent == null)
        {
            CmdImpact(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up), null, 0, false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!GetComponent<NetworkIdentity>().hasAuthority)
            return;

        Destroy(rb);

        Player _player = collision.transform.root.GetComponent<Player>();

        string _playerID = null;

        if (_player != null)
            _playerID = _player.transform.name;

        bool _stick = false;

        if (sticky && _playerID != null)
        {

            foreach (Collider _collider in colliders)
            {
                if (collision.collider.bounds.Intersects(_collider.bounds)) {

                    _playerID = _player.transform.name;

                    CmdSetParent(System.Array.IndexOf(_player.rigidbodyOnDeath, collision.collider.gameObject), _playerID, collision.collider.transform.InverseTransformPoint(transform.position));

                    for (int i = 0; i < colliders.Length; i++)
                    {
                        colliders[i].enabled = false;
                    }

                    _stick = true;

                    break;
                }
            }
       
        } 
            

        int _damage = damage;
        if (collision.collider.name == "Head")
        {
            _damage = (int)(_damage * headShotMultiplier);
        }

        CmdImpact(Quaternion.LookRotation(collision.contacts[0].normal), _playerID, _damage, _stick);
    }

    [Command]
    void CmdImpact(Quaternion _rot, string _playerID, int _damage, bool _stick)
    {

        if (_playerID != null && _playerID != playerID)
        {
            Player _player = GameManager.GetPlayer(_playerID);
            _player.RpcTakeDamage(_damage, playerID, damageType.name);
        }

        if (!_stick)
        {
            RpcImpact(_rot, playerID);
        }
    }

    [ClientRpc]
    public void RpcImpact(Quaternion _rot, string _playerID)
    {

        GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);

        DetonateExplosive _detonateExplosive = _impact.GetComponent<DetonateExplosive>();

        if (_detonateExplosive != null)
        {
            _detonateExplosive.playerID = _playerID;
            _detonateExplosive.Detonate();
        }

        Destroy(_impact, 10f);
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    void CmdSetParent(int _index, string _playerID, Vector3 _pos)
    {
        RpcSetParent(_index, _playerID, _pos);
    }

    [ClientRpc]
    void RpcSetParent(int _index, string _playerID, Vector3 _pos)
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<NetworkTransform>());

        foreach (Collider _collider in colliders)
        {
            _collider.enabled = false;
        }

        Player _player = GameManager.GetPlayer(_playerID);
        GameObject _parent = _player.rigidbodyOnDeath[_index];
            
        transform.SetParent(_parent.transform);
        transform.localPosition = _pos;
        Util.SetLayerRecursively(gameObject, _parent.layer);
    }
}

