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

    [SerializeField]
    private float fuse;

    [SerializeField]
    private bool sticky;

    private bool impacted = false;

    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        fuse -= Time.deltaTime;
        if (fuse <= 0 && transform.parent == null)
        {
            CmdImpact(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up), null, 0);
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        projectileController.rb.isKinematic = true;

        Player _player = collision.transform.root.GetComponent<Player>();
        int _rigidbodyIndex = 0;

        string _playerID = null;
        if (_player != null)
        {

            if (sticky)
            {
                foreach (Collider _collider in projectileController.colliders)
                {
                    if (collision.collider.bounds.Intersects(_collider.bounds))
                    {
                        GetComponent<NetworkTransform>().enabled = false;

                        _playerID = _player.transform.name;

                        CmdSetParent(System.Array.IndexOf(_player.rigidbodyOnDeath, collision.collider.gameObject), _playerID, collision.collider.transform.InverseTransformPoint(transform.position));

                        Util.SetLayerRecursively(gameObject, collision.collider.gameObject.layer);
                        Destroy(projectileController.rb);

                        for (int i = 0; i < projectileController.colliders.Length; i++)
                        {
                            projectileController.colliders[i].enabled = false;
                        }

                        break;
                    }
                }
            } else
            {
                _playerID = _player.transform.name;
            }
        }

        if (!impacted)
        {
            impacted = true;

            int _damage = damage;
            if (collision.collider.name == "Head")
            {
                _damage = (int)(_damage * headShotMultiplier);
            }

            CmdImpact(Quaternion.LookRotation(collision.contacts[0].normal), _playerID, _damage);
        }
    }

    [Command]
    void CmdImpact(Quaternion _rot, string _playerID, int _damage)
    {
        if (!(sticky && _playerID != null))
        {
            RpcImpact(_rot);
        }

        if (_playerID != null)
        {
            Player _player = GameManager.GetPlayer(_playerID);
            _player.RpcTakeDamage(_damage, projectileController.playerID);
        }
    }

    [ClientRpc]
    public void RpcImpact(Quaternion _rot)
    {
        impacted = true;

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
        Player _player = GameManager.GetPlayer(_playerID);
        transform.SetParent(_player.rigidbodyOnDeath[_index].transform);
        transform.localPosition = _pos;
    }
}

