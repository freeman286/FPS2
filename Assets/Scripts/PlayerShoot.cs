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

    private float timeSinceShot;

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

        if (currentWeapon.bullets < currentWeapon.magSize)
        {
            if (Input.GetButtonDown("Reload"))
            {
                weaponManager.Reload();
                return;
            }
        }

        if (currentWeapon.automatic)
        {
            if (Input.GetButtonDown("Fire1") && timeSinceShot > 1f / currentWeapon.fireRate)
            {
                CancelInvoke("Shoot");
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }

        } else
        {
            if (Input.GetButtonDown("Fire1") && timeSinceShot > 1f / currentWeapon.fireRate)
            {
                Shoot();
            }
        }

        timeSinceShot += Time.deltaTime;

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
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
        Animator anim = weaponManager.currentGraphics.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Shoot");
        }
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();

        if (weaponManager.GetCurrentCasing() != null)
        {
            GameObject _casing = (GameObject)Instantiate(weaponManager.GetCurrentCasing(), weaponManager.GetCurrentEjectionPort().transform.position, Random.rotation);
            _casing.GetComponent<Rigidbody>().velocity = weaponManager.GetCurrentEjectionPort().transform.up * Random.Range(5f, 10f);
            Destroy(_casing, 2f);
        }

        GameObject _shootSound = (GameObject)Instantiate(weaponManager.GetcurrentShootSound(), weaponManager.GetCurrentEjectionPort().transform.position, Quaternion.identity);
        Destroy(_shootSound, _shootSound.GetComponent<AudioSource>().clip.length);

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
        if (!isLocalPlayer || weaponManager.isReloading)
            return;

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
            return;
        }

        currentWeapon.bullets--;

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

        timeSinceShot = 0;

    }

    [Command]
    public void CmdPlayerShot (string _playerID, int _damage, string _sourceID)
    {

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }
}
