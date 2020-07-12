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
    public Camera weaponCam;

    [SerializeField]
    private LayerMask mask;

    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;
    private PlayerMotor motor;
    private PlayerMetrics metrics;

    //[HideInInspector]
    public float timeSinceShot = 100f;

    [HideInInspector]
    public float timeSinceScoped = 100f;

    private float timeSinceBurst = 0f;

    [HideInInspector]
    public Animator localAnim;

    [Header("Scope")]

    [SerializeField]
    private GameObject scopeUIPrefab;

    [HideInInspector]
    public GameObject scopeUIInstance;

    [HideInInspector]
    public bool isScoped;

    [SerializeField]
    private float scopeTime;

    [SerializeField]
    private float scopeCooldown;

    [SerializeField]
    public float scopedFOV;

    [HideInInspector]
    public float defaultFOV;

    private PlayerUI ui;

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
        ui = GetComponent<PlayerSetup>().ui;

        scopeUIInstance = Instantiate(scopeUIPrefab);
        scopeUIInstance.name = scopeUIPrefab.name;
        scopeUIInstance.SetActive(false);

        defaultFOV = cam.fieldOfView;
    }

    void Update()
    {
        timeSinceScoped += Time.deltaTime;
        timeSinceShot += Time.deltaTime;

        currentWeapon = weaponManager.GetCurrentWeapon();

        if (currentWeapon == null)
            return;

        if (timeSinceShot <= 1f / currentWeapon.fireRate)
        {
            timeSinceBurst += Time.deltaTime;
        }
        else if (timeSinceBurst > 0)
        {
            timeSinceBurst -= Time.deltaTime;
        }
        else
        {
            timeSinceBurst = 0;
        }

        if (currentWeapon.special)
            return;

        Recoil();

        if (currentWeapon.bullets <= 0 && timeSinceShot > 1f / currentWeapon.fireRate)
        {
            weaponManager.Reload();
        }

        if (Pause.IsOn)
        {
            if (timeSinceShot >= 1f / currentWeapon.fireRate && IsInvoking("Shoot"))
            {
                CancelInvoke("Shoot");
            }   
            return;
        }

        if (currentWeapon.bullets < currentWeapon.magSize)
        {
            if (Input.GetButtonDown("Reload"))
            {
                weaponManager.Reload();
                return;
            }
        }

        ShootInput();

        ScopeInput();
    }

    void ShootInput()
    {
        if (currentWeapon.automatic)
        {
            if (Input.GetButtonDown("Fire1") && timeSinceShot > 1f / currentWeapon.fireRate && !IsInvoking("Shoot"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }
            else if (!Input.GetButton("Fire1") && timeSinceShot >= 1f / currentWeapon.fireRate)
            {
                CancelInvoke("Shoot");
            }

        }
        else
        {
            if (Input.GetButtonDown("Fire1") && timeSinceShot > 1f / currentWeapon.fireRate)
            {
                Shoot();
            }
        }
    }

    void ScopeInput()
    {
        if (currentWeapon.scoped && localAnim != null && isLocalPlayer)
        {
            if (Input.GetButtonDown("Fire2") && timeSinceScoped > scopeCooldown)
            {
                timeSinceScoped = 0f;

                isScoped = !isScoped;

                if (isScoped)
                {
                    StartCoroutine(OnScoped());
                }
                else
                {
                    StartCoroutine(OnUnscoped());
                }
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
        if (!(isLocalPlayer && isScoped)) {
            weaponManager.GetCurrentGraphics().muzzleFlash.Play();
        }

        if (weaponManager.GetCurrentCasing() != null)
        {
            GameObject _casing = (GameObject)Instantiate(weaponManager.GetCurrentCasing(), weaponManager.GetCurrentEjectionPort().transform.position, Random.rotation);
            _casing.GetComponent<Rigidbody>().velocity = weaponManager.GetCurrentEjectionPort().transform.up * Random.Range(5f, 10f);
            Destroy(_casing, 2f);
        }

        if (!isLocalPlayer)
        {
            LocalShootEfftect();
        }

    }

    void LocalShootEfftect() // We need to do this not over the network or we'll be able to feel lag
    {
        Animator anim = weaponManager.currentGraphics.GetComponent<Animator>();
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
        {
            anim.SetTrigger("Shoot");
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
        if (!isLocalPlayer || weaponManager.isReloading || !Input.GetButton("Fire1"))
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
        } else if (isScoped)
        {
            _spread = currentWeapon.spreadWhileScoped;
        } else
        {
            _spread = currentWeapon.spreadDefault;
        }

        _spread *= currentWeapon.spreadCurve.Evaluate(timeSinceBurst / currentWeapon.timeTillMaxSpread);

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
                ProjectileShoot(_direction, _devience, currentWeapon.throwPower);
            }
            else
            {
                RaycastShoot(_direction, _devience);
            }
        }

        timeSinceShot = 0;

        LocalShootEfftect();
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

    void ProjectileShoot(Vector3 _direction, Vector3 _devience, float _velocity)
    {
        CmdProjectileShot(weaponManager.GetCurrentGraphics().firePoint.transform.position, Quaternion.LookRotation(_direction + _devience), transform.name, _velocity);
    }

    [Command]
    public void CmdPlayerShot (string _playerID, int _damage, string _sourceID)
    {

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }

    [Command]
    void CmdProjectileShot(Vector3 _pos, Quaternion _rot, string _playerID, float _velocity)
    {
        GameObject _projectile = (GameObject)Instantiate(weaponManager.GetCurrentProjectile(), _pos, _rot);
        NetworkServer.Spawn(_projectile, connectionToClient);
        

        ProjectileController _projectileController = _projectile.GetComponent<ProjectileController>();

        _projectileController.playerID = _playerID;
        _projectileController.RpcLaunch(_velocity);
    }

    void Recoil()
    {
        if (!isLocalPlayer || currentWeapon == null)
            return;

        float _recoil = Mathf.Clamp(currentWeapon.recoilTime - timeSinceShot, 0, currentWeapon.recoilTime) * currentWeapon.recoilAmount * Time.deltaTime;

        if (_recoil > 0)
        {
            motor.AddRotation(new Vector3(0, Random.Range(-currentWeapon.horizontalRecoilMultiplier, currentWeapon.horizontalRecoilMultiplier) * _recoil, 0));
            motor.AddRotationCamera(_recoil);
        }
    }

    private IEnumerator OnScoped()
    {
        localAnim.SetBool("Scoped", true);

        yield return new WaitForSeconds(scopeTime);
        scopeUIInstance.SetActive(true);
        ui.crosshair.SetActive(false);
        weaponCam.enabled = false;

        Environment.instance.Scope();
        cam.fieldOfView = scopedFOV;
    }

    private IEnumerator OnUnscoped()
    {
        localAnim.SetBool("Scoped", false);
        yield return new WaitForSeconds(scopeTime);
        Unscope();
    }

    public void Unscope()
    {
        scopeUIInstance.SetActive(false);
        ui.crosshair.SetActive(true);
        weaponCam.enabled = true;
        cam.fieldOfView = defaultFOV;
        Environment.instance.UnScope();
    }
}
