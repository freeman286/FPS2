using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(ProjectileController))]
public class ImpactController : NetworkBehaviour
{
    private ProjectileController projectileController;

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

    [HideInInspector]
    public float timeSinceCreated = 0f;

    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        timeSinceCreated += Time.deltaTime;
        if (fuse <= timeSinceCreated && transform.parent == null)
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
            _player.RpcTakeDamage(_damage, projectileController.playerID, damageType.name);
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

        foreach (Collider _collider in projectileController.colliders)
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

