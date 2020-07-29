using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUtil : MonoBehaviour
{
    public static Equipment[] AllEquipment()
    {
        List<Equipment> allEquipment = new List<Equipment>();

        string[] paths = Util.GetSubAllFilesInDirectory("Assets/Resources/ScriptableObjects/Equipment", "asset");

        foreach (string _path in paths)
        {
            string _equipmentPath = _path.Substring(17, _path.Length - 23); // Remove extra characters from the path we don't need
            Equipment _equipment = (Equipment)Resources.Load<Equipment>(_equipmentPath);

            allEquipment.Add(_equipment);
        }

        return allEquipment.ToArray();
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
