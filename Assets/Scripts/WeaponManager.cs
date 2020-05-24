using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private PlayerShoot shoot;


    [SerializeField]
    private Transform weaponHolder;

    private PlayerWeapon primaryWeapon;

    private PlayerWeapon secondaryWeapon;

    private PlayerWeapon currentWeapon;
    public WeaponGraphics currentGraphics;
    [SyncVar]
    public string currentWeaponName;

    public bool isReloading = false;

    private PlayerWeapon[] allWeapons;

    private Animator anim;

    public float switchingTime;

    private bool switchingWeapon = false;

    private IEnumerator reload;


    void Start()
    {
        allWeapons = Util.AllWeapons();
        SetDefaults();
        anim = GetComponent<Animator>();
    }   

    void Update()
    {
        if (Input.GetButtonDown("Switch") && isLocalPlayer && !switchingWeapon)
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
            primaryWeapon = NameToWeapon(PlayerInfo.primaryWeaponName);
            secondaryWeapon = NameToWeapon(PlayerInfo.secondaryWeaponName);

            primaryWeapon.Load();
            secondaryWeapon.Load();
            EquipWeapon(primaryWeapon, true);
        }
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
        PlayerWeapon newWeapon = NameToWeapon(_weaponName);

        currentWeapon = newWeapon;

        if (!_setup && anim != null)
        {
            anim.SetTrigger("Switching");
        }

        StartCoroutine(ShowWeapon());
    }

    private IEnumerator ShowWeapon()
    {

        yield return new WaitForSeconds(0.1f);

        foreach (Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }

        GameObject _weaponIns = (GameObject)Instantiate(currentWeapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = _weaponIns.GetComponent<WeaponGraphics>();
        if (currentGraphics == null)
            Debug.LogError("No WeaponGraphics component on weapon object: " + _weaponIns.name);

        if (isLocalPlayer)
            Util.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer(weaponLayerName));
    }

    public void Reload()
    {
        if (isReloading)
            return;

        reload = Reload_Coroutine();
        StartCoroutine(reload);
    }

    private IEnumerator Reload_Coroutine()
    {
        isReloading = true;

        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.Load();

        anim.enabled = false;
        anim.enabled = true;

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
        }
    }

    public PlayerWeapon NameToWeapon(string _name)
    {
        foreach (var weapon in allWeapons)
        {
            if (weapon.name == _name)
            {
                return weapon;
            }
        }
        return null;
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
}