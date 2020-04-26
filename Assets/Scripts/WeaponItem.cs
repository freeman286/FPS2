using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponItem : MonoBehaviour
{

    [SerializeField]
    private Text nameText;

    private WeaponList weaponList;

    public string weaponName;

    public bool selected = false;

    public void Setup(PlayerWeapon _weapon, WeaponList _weaponList)
    {
        weaponName = _weapon.name;
        nameText.text = weaponName;
        weaponList = _weaponList;
    }

    public void Deselect()
    {
        selected = false;
        GetComponent<Button>().interactable = true;
    }

    public void Select()
    {
        selected = true;
        GetComponent<Button>().interactable = false;
    }

    public void SelectWeapon()
    {
        weaponList.Select(weaponName);
    }

}
