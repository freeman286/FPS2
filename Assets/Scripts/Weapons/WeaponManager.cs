using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private ListType[] listTypes = new ListType[2];

    [SerializeField]
    private Transform weaponHolder = null;

    [HideInInspector]
    public PlayerWeapon primaryWeapon;

    [HideInInspector]
    public PlayerWeapon secondaryWeapon;

    public PlayerWeapon currentWeapon;

    public PlayerWeaponGraphics currentGraphics;
    [SyncVar]
    public string currentWeaponName;

    private PlayerShoot shoot;
    private PlayerStats stats;
    private RaycastShoot raycastShoot;
    private PlayerMetrics metrics;
    private PlayerEquipment equipment;
    private Animator anim;

    public bool isReloading = false;

    private PlayerWeapon[] allWeapons;

    public float switchingTime;

    private bool switchingWeapon = false;

    private IEnumerator reload;

    private GameObject weaponIns;

    private Behaviour[] scripts;


    void Start()
    {
        allWeapons = WeaponsUtil.AllWeapons();
        shoot = GetComponent<PlayerShoot>();
        raycastShoot = GetComponent<RaycastShoot>();
        metrics = GetComponent<PlayerMetrics>();
        stats = GetComponent<PlayerStats>();
        equipment = GetComponent<PlayerEquipment>();
        anim = GetComponent<Animator>();

        Player _player = GetComponent<Player>();
        _player.onPlayerSetDefaultsCallback += SetDefaults;
        _player.onPlayerDieCallback += Die;

    }   

    void Update()
    {
        if (Input.GetButtonDown("Switch") && isLocalPlayer && !switchingWeapon && !Pause.IsOn)
        {
            if (reload != null)
            {
                StopCoroutine(reload);
                isReloading = false;
            }

            SwitchWeapon();
            switchingWeapon = true;
        }
    }

    public void SetDefaults()
    {
        if (isLocalPlayer)
        {
            primaryWeapon = WeaponsUtil.NameToWeapon(PlayerInfo.GetNameSelected(listTypes[0]));
            secondaryWeapon = WeaponsUtil.NameToWeapon(PlayerInfo.GetNameSelected(listTypes[1]));

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
            player.weaponManager.LocalEquipWeapon(player.weaponManager.currentWeaponName, true);
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
        equipment.enabled = false;

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
        equipment.enabled = true;

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

    void EquipWeapon(PlayerWeapon _weapon, bool _setup)
    {
        currentWeapon = _weapon;

        EnableScripts();

        if (shoot != null)
        {
            shoot.Unscope();
            shoot.isScoped = false;
        }

        CmdEquipWeapon(transform.name, _weapon.name, _setup);
    }

    void DisableScripts()
    {
        if (scripts != null)
        {
            for (int i = 0; i < scripts.Length; i++)
            {
                scripts[i].enabled = false;
            }
        }
    }

    void EnableScripts()
    {

        DisableScripts();

        if (currentWeapon.scriptsToEnable != null)
        {
            scripts = new Behaviour[currentWeapon.scriptsToEnable.Length];
            for (int i = 0; i < currentWeapon.scriptsToEnable.Length; i++)
            {
                scripts[i] = Util.EnableScipt(gameObject, currentWeapon.scriptsToEnable[i], true);
            }
        }
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
        LocalEquipWeapon(_weaponName, _setup);
    }

    public void LocalEquipWeapon(string _weaponName, bool _setup)
    {
        PlayerWeapon newWeapon = WeaponsUtil.NameToWeapon(_weaponName);

        currentWeapon = newWeapon;

        if (!_setup && anim != null)
        {
            anim.SetTrigger("Switching");
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

        if (weaponHolder.childCount > 0 || currentWeapon == null) {
            yield break;
        }

        weaponIns = (GameObject)Instantiate(currentWeapon.gameObject, weaponHolder.position, weaponHolder.rotation);
        weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = weaponIns.GetComponent<PlayerWeaponGraphics>();
        if (currentGraphics == null)
            Debug.LogError("No WeaponGraphics component on weapon object: " + weaponIns.name);

        raycastShoot.hitEffectPrefab = currentGraphics.hitEffectPrefab;

        if (isLocalPlayer)
            Util.SetLayerRecursively(weaponIns, LayerMask.NameToLayer(weaponLayerName));

        for (int i = 0; i < currentGraphics.colliders.Length; i++)
        {
            currentGraphics.colliders[i].enabled = false;
        }

        shoot.localAnim = currentGraphics.GetComponent<Animator>();
    }

    public void Reload()
    {
        if (isReloading)
            return;

        currentGraphics.animator.ResetTrigger("Shoot");
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
        currentGraphics.animator.SetTrigger("Reload");
        StartCoroutine(CancelShoot_Coroutine());
    }

    private IEnumerator CancelShoot_Coroutine()
    {
        yield return new WaitForSeconds(1/currentWeapon.fireRate);
        currentGraphics.animator.ResetTrigger("Shoot"); // Sometimes the shoot trigger is still set and a phantom animation could play
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
        if (currentWeapon == null)
            return 1;

        return shoot.isScoped ? currentWeapon.scopedSpeed : currentWeapon.speed;
    }

    public void Die()
    {

        currentGraphics.light.enabled = false;

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
            DisableScripts();
        }
    }
}