using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponList : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponItemPrefab = null;

    [SerializeField]
    private bool primary = true;

    private PlayerWeapon[] allWeapons;

    private WeaponItem[] items;

    private PlayerInfo playerInfo;

    public string selectedWeaponName;

    void Start()
    {
        allWeapons = Util.AllWeapons().Where(w => w.primary == primary).ToArray(); ;
        items = new WeaponItem[allWeapons.Length];

        for (int i = 0; i < allWeapons.Length; i++)
        {
            GameObject go = (GameObject)Instantiate(weaponItemPrefab, this.transform);
            WeaponItem item = go.GetComponent<WeaponItem>();
            item.Setup(allWeapons[i], this);
            items[i] = item;
        }

        if (string.IsNullOrEmpty(PlayerInfo.primaryWeaponName) && primary)
        {
            PlayerInfo.primaryWeaponName = allWeapons[0].name;
        }

        if (string.IsNullOrEmpty(PlayerInfo.secondaryWeaponName) && !primary)
        {
            PlayerInfo.secondaryWeaponName = allWeapons[0].name;
        }


        Select(primary ? PlayerInfo.primaryWeaponName : PlayerInfo.secondaryWeaponName);
    }

    public void Select(string _weaponName)
    {
        selectedWeaponName = _weaponName;

        if (primary)
        {
            PlayerInfo.primaryWeaponName = _weaponName;
        } else
        {
            PlayerInfo.secondaryWeaponName = _weaponName;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (_weaponName == items[i].weaponName)
            {
                items[i].Select();
            }
            else
            {
                items[i].Deselect();
            }
        }
    }
}
