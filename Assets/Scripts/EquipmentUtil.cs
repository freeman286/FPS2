using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EquipmentUtil : MonoBehaviour
{
    public static Equipment[] AllEquipment()
    {
        List<Equipment> allEquipment = new List<Equipment>();

        string[] paths = Util.GetSubAllFilesInDirectory("Assets/Resources/ScriptableObjects/Equipment", "asset");

        foreach (string _path in paths)
        {
            Debug.Log(_path);
            allEquipment.Add((Equipment)AssetDatabase.LoadAssetAtPath(_path, typeof(Equipment)));
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
