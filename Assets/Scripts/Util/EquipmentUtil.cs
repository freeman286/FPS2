using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUtil : MonoBehaviour
{
    public static Equipment[] AllEquipment()
    {
        Equipment[] allEquipment = Resources.LoadAll<Equipment>("ScriptableObjects/Equipment");

        return allEquipment;
    }

    public static Equipment NameToEquipment(string _name)
    {
        Equipment[] allEquipment = AllEquipment();

        foreach (var equipment in allEquipment)
        {
            if (equipment.name == _name)
            {
                return equipment;
            }
        }
        return null;
    }

}
