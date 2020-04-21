using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerWeapon primaryWeapon;

    [SerializeField]
    private PlayerWeapon secondaryWeapon;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;
    [SyncVar]
    public string currentWeaponName;

    public bool isReloading = false;

    private PlayerWeapon[] allWeapons;

    void Start()
    {
        Object[] allWeaponObjects = Resources.LoadAll("Prefabs/Weapons", typeof(GameObject));

        allWeapons = new PlayerWeapon[allWeaponObjects.Length];

        for (int i = 0; i < allWeaponObjects.Length; i++)
        {
            allWeapons[i] = ((GameObject)allWeaponObjects[i]).GetComponent<PlayerWeapon>();
        }

        primaryWeapon.Load();
        secondaryWeapon.Load();
        EquipWeapon(primaryWeapon);
    }

    void Update()
    {
        if (Input.GetButtonDown("Switch") && !isReloading && isLocalPlayer)
        {
            SwitchWeapon();
        }
    }

    public void SyncAllWepaons()
    {

        foreach (var player in GameManager.GetAllPlayers())
        {
            CmdEquipWeaponGraphics(player.name, player.weaponManager.currentWeaponName);
        }

    }

    void SwitchWeapon()
    {
        if (currentWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        } else
        {
            EquipWeapon(primaryWeapon);
        }
    }

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    [Client]
    void EquipWeapon(PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;

        CmdEquipWeaponGraphics(transform.name, _weapon.name);
    }

    [Command]
    void CmdEquipWeaponGraphics(string _playerID, string _weaponName)
    {
        currentWeaponName = _weaponName;
        GameManager.GetPlayer(_playerID).weaponManager.RpcEquipWeaponGraphics(_weaponName);
    }

    [ClientRpc]
    void RpcEquipWeaponGraphics(string _weaponName)
    {
        PlayerWeapon newWeapon = NameToWeapon(_weaponName);

        currentWeapon = newWeapon;

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

        StartCoroutine(Reload_Coroutine());
    }

    private IEnumerator Reload_Coroutine()
    {
        isReloading = true;

        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

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
}