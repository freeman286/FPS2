using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder = null;

    [HideInInspector]
    public PlayerWeapon primaryWeapon;

    [HideInInspector]
    public PlayerWeapon secondaryWeapon;

    public PlayerWeapon currentWeapon;

    public WeaponGraphics currentGraphics;
    [SyncVar]
    public string currentWeaponName;

    private PlayerShoot shoot;

    private PlayerStats stats;

    public bool isReloading = false;

    private PlayerWeapon[] allWeapons;

    private Animator anim;

    public float switchingTime;

    private bool switchingWeapon = false;

    private IEnumerator reload;

    private GameObject weaponIns;

    private PlayerMetrics metrics;

    private Behaviour[] scripts;


    void Start()
    {
        allWeapons = WeaponsUtil.AllWeapons();
        shoot = GetComponent<PlayerShoot>();
        anim = GetComponent<Animator>();
        metrics = GetComponent<PlayerMetrics>();
        stats = GetComponent<PlayerStats>();

        SetDefaults();
    }   

    void Update()
    {
        if (Input.GetButtonDown("Switch") && isLocalPlayer && !switchingWeapon && !Pause.IsOn)
        {
            if (reload != null)
                StopCoroutine(reload);
                isReloading = false;

            SwitchWeapon();
            switchingWeapon = true;
        }
    }

    public void SetDefaults()
    {
        if (isLocalPlayer)
        {
            primaryWeapon = WeaponsUtil.NameToWeapon(PlayerInfo.primaryWeaponName);
            secondaryWeapon = WeaponsUtil.NameToWeapon(PlayerInfo.secondaryWeaponName);

            

            primaryWeapon.Load();
            secondaryWeapon.Load();
            EquipWeapon(primaryWeapon, true);

            shoot.CancelInvoke("Shoot");

            if (shoot != null)
            {
                shoot.Unscope();
            }
        }

        stats.GetSets();
    }

    public void SyncAllWepaons()
    {
        foreach (var player in GameManager.GetAllPlayers())
        {
            CmdEquipWeapon(player.name, player.weaponManager.currentWeaponName, true);
        }
    }

    void SwitchWeapon()
    {
        StartCoroutine(SwitchWeapon_Coroutine());
    }

    private IEnumerator SwitchWeapon_Coroutine()
    {
        shoot.CancelInvoke("Shoot");
        shoot.enabled = false;

        if (currentWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon, false);
        }
        else
        {
            EquipWeapon(primaryWeapon, false);
        }

        yield return new WaitForSeconds(switchingTime);

        shoot.enabled = true;
        switchingWeapon = false;
    }

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public GameObject GetCurrentProjectile()
    {
        return currentWeapon.projectile;
    }


    [Client]
    void EquipWeapon(PlayerWeapon _weapon, bool _setup)
    {
        currentWeapon = _weapon;

        if (shoot != null)
        {
            shoot.Unscope();
            shoot.isScoped = false;
        }

        CmdEquipWeapon(transform.name, _weapon.name, _setup);
    }

    [Command]
    void CmdEquipWeapon(string _playerID, string _weaponName, bool _setup)
    {
        currentWeaponName = _weaponName;
        GameManager.GetPlayer(_playerID).weaponManager.RpcEquipWeapon(_weaponName, _setup);
    }

    [ClientRpc]
    void RpcEquipWeapon(string _weaponName, bool _setup)
    {
        PlayerWeapon newWeapon = WeaponsUtil.NameToWeapon(_weaponName);

        currentWeapon = newWeapon;

        if (!_setup && anim != null)
        {
            anim.SetTrigger("Switching");
        }

        if (scripts != null)
        {
            for (int i = 0; i < scripts.Length; i++)
            {
                scripts[i].enabled = false;
            }
        }

        StartCoroutine(ShowWeapon());
    }

    private IEnumerator ShowWeapon()
    {
        foreach (Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitForEndOfFrame();

        if (weaponHolder.childCount > 0) {
            yield break;
        }

        weaponIns = (GameObject)Instantiate(currentWeapon.gameObject, weaponHolder.position, weaponHolder.rotation);
        weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = weaponIns.GetComponent<WeaponGraphics>();
        if (currentGraphics == null)
            Debug.LogError("No WeaponGraphics component on weapon object: " + weaponIns.name);

        if (isLocalPlayer)
            Util.SetLayerRecursively(weaponIns, LayerMask.NameToLayer(weaponLayerName));

        for (int i = 0; i < currentGraphics.colliders.Length; i++)
        {
            currentGraphics.colliders[i].enabled = false;
        }

        shoot.localAnim = currentGraphics.GetComponent<Animator>();

        if (currentWeapon.scriptsToEnable != null)
        {
            scripts = new Behaviour[currentWeapon.scriptsToEnable.Length];
            for (int i = 0; i < currentWeapon.scriptsToEnable.Length; i++)
            {
                scripts[i] = Util.EnableScipt(gameObject, currentWeapon.scriptsToEnable[i], true);
            }
        }
    }

    public void Reload()
    {
        if (isReloading)
            return;

        anim.ResetTrigger("Shoot");
        reload = Reload_Coroutine();
        StartCoroutine(reload);
    }

    private IEnumerator Reload_Coroutine()
    {
        isReloading = true;

        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime + Mathf.Clamp(1f / currentWeapon.fireRate - shoot.timeSinceShot, 0, 1f / currentWeapon.fireRate));

        currentWeapon.Load();

        isReloading = false;
    }

    [Command]
    void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    void RpcOnReload()
    {
        Animator anim = currentGraphics.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Reload");
            StartCoroutine(CancelShoot_Coroutine());
        }
    }

    private IEnumerator CancelShoot_Coroutine()
    {
        yield return new WaitForSeconds(1/currentWeapon.fireRate);
        anim.ResetTrigger("Shoot"); // Sometimes the shoot trigger is still set and a phantom animation could play
    }

    public GameObject GetCurrentCasing()
    {
        return currentGraphics.casing;
    }

    public GameObject GetCurrentEjectionPort()
    {
        return currentGraphics.ejectionPort;
    }


    public GameObject GetcurrentShootSound()
    {
        return currentGraphics.shootSound;
    }

    public float GetCurrentSpeed()
    {
        return shoot.isScoped ? currentWeapon.scopedSpeed : currentWeapon.speed;

    }

    public void Die()
    {
        // Change the current weapon into a prop

        for (int i = 0; i < currentGraphics.colliders.Length; i++)
        {
            if (currentGraphics.colliders[i] != null)
            {
                currentGraphics.colliders[i].enabled = true;
            }
        }

        Rigidbody rigidbody = weaponIns.AddComponent<Rigidbody>();
        rigidbody.mass = 0.5f;
        rigidbody.drag = 1;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.velocity = metrics.velocity;

        Util.SetLayerRecursively(weaponIns, LayerMask.NameToLayer("Prop"));
        Destroy(weaponIns.GetComponent<Animator>());

        if (isLocalPlayer)
        {
            shoot.Unscope(); // Scope out
            shoot.weaponCam.enabled = false;
        }
    }
}