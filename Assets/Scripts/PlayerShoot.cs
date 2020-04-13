using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;


    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("PlayerShoot: No camera referenced!");
                this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (Pause.IsOn)
            return;

        if (currentWeapon.fireRate <= 0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        } else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate);
            } else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }
    }

    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEfftect();
    }

    [ClientRpc]
    void RpcDoShootEfftect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play(); 
    }

    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEfftect(_pos, _normal);
    }

    [ClientRpc]
    void RpcDoHitEfftect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    void Shoot()
    {
        if (!isLocalPlayer)
            return;

        CmdOnShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            if (_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.transform.root.name, currentWeapon.damage, transform.name);
            }

            CmdOnHit(_hit.point, _hit.normal);

        }
    }

    [Command]
    public void CmdPlayerShot (string _playerID, int _damage, string _sourceID)
    {
        Debug.Log(_playerID + " has been shot");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }
}
