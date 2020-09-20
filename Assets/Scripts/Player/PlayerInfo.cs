using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance;

    static public string ipAddress = "localhost";

    private static Dictionary<ListType, string> loadoutNames = new Dictionary<ListType, string>();

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one PlayerInfo in scene.");
        }
        else
        {
            instance = this;
        }
    }

    public static string GetNameSelected(ListType _listType)
    {
        if (loadoutNames.ContainsKey(_listType))
        {
            return loadoutNames[_listType];
        } else
        {
            return null;
        }
    }

    public static void SelectName(ListType _listType, string _name)
    {
        if (loadoutNames.ContainsKey(_listType))
        {
            loadoutNames[_listType] = _name;
        }
        else
        {
            loadoutNames.Add(_listType, _name);
        }
    }
}
