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
            CmdImpact(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up));
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
            _damage = (int)(_damage * headShotMultiplier);

        if (!_stick)
            CmdImpact(Quaternion.LookRotation(collision.contacts[0].normal));

        Health _health = collision.transform.root.GetComponent<Health>();

        if (_health != null)
            CmdDamage(_health.transform.name, _damage, playerID, damageType.name);
    }

    [Command]
    void CmdDamage(string _healthID, int _damage, string _sourceID, string _damageType)
    {
        Health _health = GameManager.GetHealth(_healthID);
        _health.RpcTakeDamage(_damage, _sourceID, _damageType);
    }


    [Command]
    void CmdImpact(Quaternion _rot)
    {
        RpcImpact(_rot);
    }

    [ClientRpc]
    public void RpcImpact(Quaternion _rot)
    {

        GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);

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

