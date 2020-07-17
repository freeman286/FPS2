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
    private float headShotMultiplier;

    public float fuse;

    [SerializeField]
    private bool sticky;

    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        fuse -= Time.deltaTime;
        if (fuse <= 0 && transform.parent == null)
        {
            CmdImpact(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up), null, 0, false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!GetComponent<NetworkIdentity>().hasAuthority)
            return;

        Destroy(projectileController.rb);

        Player _player = collision.transform.root.GetComponent<Player>();
        int _rigidbodyIndex = 0;

        string _playerID = null;

        if (_player != null)
            _playerID = _player.transform.name;

        bool _stick = false;

        if (sticky && _playerID != null && collision.collider.bounds.IntersectRay(new Ray(transform.position, transform.forward)))
        {
                                            
            _playerID = _player.transform.name;

            CmdSetParent(System.Array.IndexOf(_player.rigidbodyOnDeath, collision.collider.gameObject), _playerID, collision.collider.transform.InverseTransformPoint(transform.position));

            for (int i = 0; i < projectileController.colliders.Length; i++)
            {
                projectileController.colliders[i].enabled = false;
            }

            _stick = true;
       
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

        if (_playerID != null)
        {
            Player _player = GameManager.GetPlayer(_playerID);
            _player.RpcTakeDamage(_damage, projectileController.playerID);
        }

        if (!_stick)
        {
            RpcImpact(_rot);
        }
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

        Player _player = GameManager.GetPlayer(_playerID);
        GameObject _parent = _player.rigidbodyOnDeath[_index];
            
        transform.SetParent(_parent.transform);
        transform.localPosition = _pos;
        Util.SetLayerRecursively(gameObject, _parent.layer);
    }
}

