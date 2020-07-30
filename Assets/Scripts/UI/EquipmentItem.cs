using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentItem : MonoBehaviour
{
    [SerializeField]
    private Text nameText = null;

    private EquipmentList equipmentList;

    public string equipmentName;

    public bool selected = false;

    public void Setup(Equipment _equipment, EquipmentList _equipmentList)
    {
        equipmentName = _equipment.name;
        nameText.text = equipmentName;
        equipmentList = _equipmentList;
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

    public void SelectEquipment()
    {
        equipmentList.Select(equipmentName);
    }
}
