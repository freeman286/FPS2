using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponManager), typeof(RaycastShoot), typeof(ProjectileShoot))] 
public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player";
    private const string PROJECTILE_TAG = "Projectile";
    private const string EQUIPMENT_TAG = "Equipment";

    [SerializeField]
    private Camera cam = null;

    [SerializeField]
    public Camera weaponCam = null;

    public LayerMask mask;

    private PlayerWeapon currentWeapon;

    private WeaponManager weaponManager;
    private PlayerMotor motor;
    private PlayerMetrics metrics;

    //Shooting helper scripts
    private RaycastShoot raycastShoot;
    private ProjectileShoot projectileShoot;
    private ShootEffects shootEffects;

    [HideInInspector]
    public float timeSinceShot = Mathf.Infinity;

    [HideInInspector]
    public float timeSinceScoped = Mathf.Infinity;

    private float timeSinceBurst = 0f;

    [HideInInspector]
    public Animator localAnim;

    [Header("Scope")]

    [SerializeField]
    private GameObject scopeUIPrefab = null;

    [HideInInspector]
    public GameObject scopeUIInstance = null;

    [HideInInspector]
    public bool isScoped;

    [SerializeField]
    private float scopeTime = 0.15f;

    [SerializeField]
    private float scopeCooldown = 1f;

    [SerializeField]
    public float scopedFOV = 15f;

    [HideInInspector]
    public float defaultFOV = 88f;

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

        raycastShoot = GetComponent<RaycastShoot>();
        projectileShoot = GetComponent<ProjectileShoot>();
        shootEffects = GetComponent<ShootEffects>();

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

        if (currentWeapon.bullets <= 0 && CanShoot())
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
            if (Input.GetButtonDown("Fire1") && CanShoot() && !IsInvoking("Shoot"))
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
            if (Input.GetButtonDown("Fire1") && CanShoot())
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
    public void CmdOnShoot()
    {
        RpcDoShootEfftect();
    }

    [ClientRpc]
    void RpcDoShootEfftect()
    {
        if (weaponManager.GetCurrentCasing() != null)
        {
            GameObject _casing = (GameObject)Instantiate(weaponManager.GetCurrentCasing(), weaponManager.GetCurrentEjectionPort().transform.position, Random.rotation);
            _casing.GetComponent<Rigidbody>().velocity = weaponManager.GetCurrentEjectionPort().transform.up * Random.Range(5f, 10f);
            Destroy(_casing, 2f);
        }

        if (!isLocalPlayer)
        {
            shootEffects.LocalShootEfftect((WeaponGraphics)weaponManager.GetCurrentGraphics());
        }
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

        Vector3 _direction = raycastShoot.ShootDirection(cam.transform, weaponManager.GetCurrentGraphics().firePoint.transform, currentWeapon.range, mask);
        
        if (currentWeapon.projectile != null)
        {
            projectileShoot.Shoot(weaponManager.GetCurrentGraphics().firePoint.transform, _direction, _devience, currentWeapon.throwPower, currentWeapon.projectile, transform.name, currentWeapon.roundsPerShot);
        }
        else
        {
            raycastShoot.Shoot(weaponManager.GetCurrentGraphics().firePoint.transform, _direction, _devience, currentWeapon, mask, transform.name);
        }

        timeSinceShot = 0;

        shootEffects.LocalShootEfftect((WeaponGraphics)weaponManager.GetCurrentGraphics());
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

    public bool CanShoot()
    {
        return timeSinceShot > 1f / currentWeapon.fireRate;
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
