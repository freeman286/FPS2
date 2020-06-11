using Mirror;
using System.Collections;
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
    private PlayerMotor motor;
    private PlayerMetrics metrics;

    private float timeSinceShot = 100f;

    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("PlayerShoot: No camera referenced!");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
        motor = GetComponent<PlayerMotor>();
        metrics = GetComponent<PlayerMetrics>();
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
            else if (!Input.GetButton("Fire1") || Input.GetButton("Cancel"))
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

        Recoil();
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
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
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

        float _spread;

        if (!metrics.IsGrounded())
        {
            _spread = currentWeapon.spreadWhileJumping;
        } else if (metrics.IsMoving())
        {
            _spread = currentWeapon.spreadWhileMoving;
        } else
        {
            _spread = currentWeapon.spreadDefault;
        }

        Vector3 _devience = Random.insideUnitSphere * _spread;

        Vector3 _direction;

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position + cam.transform.forward * 2f, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            _direction = (_hit.point - weaponManager.GetCurrentGraphics().firePoint.transform.position).normalized; 
        }
        else
        {
            _direction = (cam.transform.position + cam.transform.forward * currentWeapon.range - weaponManager.GetCurrentGraphics().firePoint.transform.position).normalized;
        }

        for (int i = 0; i < currentWeapon.roundsPerShot; i++)
        {
            if (currentWeapon.projectileWeapon)
            {
                ProjectileShoot(_direction, _devience);
            }
            else
            {
                RaycastShoot(_direction, _devience);
            }
        }

        timeSinceShot = 0;

    }

    void RaycastShoot(Vector3 _direction, Vector3 _devience)
    {
        Vector3 _cone = Random.insideUnitSphere * currentWeapon.coneOfFire;

        RaycastHit _hit;
        if (Physics.Raycast(weaponManager.GetCurrentGraphics().firePoint.transform.position + _direction * 0.2f, _direction + _devience + _cone, out _hit, currentWeapon.range, mask))
        {

            int _damage = Mathf.RoundToInt(currentWeapon.damageFallOff.Evaluate(_hit.distance / currentWeapon.range) * currentWeapon.damage);

            Rigidbody rb = _hit.collider.attachedRigidbody;

            if (rb != null && rb.GetComponent<Player>() == null)
                rb.AddForceAtPosition((_direction + _devience + _cone) * _damage, _hit.point);

            if (_hit.collider.tag == PLAYER_TAG)
            {
                
                if (_hit.collider.transform.name == "Head")
                {
                    _damage = (int)(_damage * currentWeapon.headShotMultiplier);
                }

                CmdPlayerShot(_hit.collider.transform.root.name, _damage, transform.name);
            }
            

            CmdOnHit(_hit.point, _hit.normal);

        }
    }

    void ProjectileShoot(Vector3 _direction, Vector3 _devience)
    {
        CmdProjectileShot(weaponManager.GetCurrentGraphics().firePoint.transform.position, Quaternion.LookRotation(_direction + _devience), transform.name);
    }

    [Command]
    public void CmdPlayerShot (string _playerID, int _damage, string _sourceID)
    {

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }

    [Command]
    void CmdProjectileShot(Vector3 _pos, Quaternion _rot, string _playerID)
    {
        GameObject _projectile = (GameObject)Instantiate(weaponManager.GetCurrentProjectile(), _pos, _rot);
        NetworkServer.Spawn(_projectile, connectionToClient);
        _projectile.GetComponent<ProjectileController>().playerID = _playerID;
    }

    void Recoil()
    {
        
        float _recoil = Mathf.Clamp(currentWeapon.recoilTime - timeSinceShot, 0, currentWeapon.recoilTime) * currentWeapon.recoilAmount * Time.deltaTime;
        if (_recoil > 0)
        {
            motor.AddRotation(new Vector3(0, Random.Range(-1.0f, 1.0f) * _recoil, 0));
            motor.AddRotationCamera(_recoil);
        }
    }
}
