using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EquipmentList : MonoBehaviour
{
    [SerializeField]
    private GameObject equipmentItemPrefab = null;

    private Equipment[] allEquipment;

    private EquipmentItem[] items;

    private PlayerInfo playerInfo;

    public string selectedEquipmentWeaponName;

    void Start()
    {
        allEquipment = EquipmentUtil.AllEquipment();
        items = new EquipmentItem[allEquipment.Length];

        for (int i = 0; i < allEquipment.Length; i++)
        {
            GameObject go = (GameObject)Instantiate(equipmentItemPrefab, this.transform);
            EquipmentItem item = go.GetComponent<EquipmentItem>();
            item.Setup(allEquipment[i], this);
            items[i] = item;
        }

        if (string.IsNullOrEmpty(PlayerInfo.equipmentName))
        {
            PlayerInfo.equipmentName = allEquipment[0].name;
        }

        Select(PlayerInfo.equipmentName);
    }

    public void Select(string _equipmentName)
    {
        selectedEquipmentWeaponName = _equipmentName;

        PlayerInfo.equipmentName = _equipmentName;

        for (int i = 0; i < items.Length; i++)
        {
            if (_equipmentName == items[i].equipmentName)
            {
                items[i].Select();
            }
            else
            {
                items[i].Deselect();
            }
        }

        SetList.instance.ShowSets();
    }
}

