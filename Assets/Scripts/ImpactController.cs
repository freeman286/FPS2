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
        if (fuse <= 0 && !impacted)
        {
            CmdImpact(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up), null, 0);
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
        if (sticky && _playerID != null)
        { 
            RpcStick(_playerID, _rot);
        } else
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

    [ClientRpc]
    public void RpcStick(string _playerID, Quaternion _rot)
    {
        impacted = true;

        Player _player = GameManager.GetPlayer(_playerID);

        Transform _stick = null;
        float _target = 0.6f;

        Vector3 _centre;
        if (GetComponent<CentreOfMass>() == null)
        {
            _centre = transform.position;
        } else
        {
            _centre = transform.TransformPoint(GetComponent<CentreOfMass>().centre);
        }

        for(int i = 0; i < _player.rigidbodyOnDeath.Length; i++)
        {
            float _distance = Vector3.Distance(_player.rigidbodyOnDeath[i].transform.position, _centre);
            if (_distance < _target)
            {
                _stick = _player.rigidbodyOnDeath[i].transform;
                _target = _distance;
            }
        }

        if (_stick == null)
        {
            NetworkServer.Destroy(gameObject);
            GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);
            Destroy(_impact, 10f);
            return;
        }

        transform.SetParent(_stick);
        Util.SetLayerRecursively(gameObject, _stick.gameObject.layer);
        Destroy(projectileController.rb);

        for (int i = 0; i < projectileController.colliders.Length; i++)
        {
            projectileController.colliders[i].enabled = false;
        }
    }

}

