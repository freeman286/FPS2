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

    public bool isReloading = false;

    void Start()
    {
        EquipWeapon(primaryWeapon);
    }

    void Update()
    {
        if (Input.GetButtonDown("Switch") && !isReloading && isLocalPlayer)
        {
            SwitchWeapon();
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

        CmdEquipWeaponGraphics();
        
    }

    [Command]
    void CmdEquipWeaponGraphics()
    {
        RpcEquipWeaponGraphics();
    }

    [ClientRpc]
    void RpcEquipWeaponGraphics()
    {

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

        currentWeapon.bullets = currentWeapon.magSize;

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
}