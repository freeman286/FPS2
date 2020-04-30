using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Util
{

    public static void SetLayerRecursively(GameObject _obj, int _newLayer)
    {
        if (_obj == null)
            return;
        _obj.layer = _newLayer;

        foreach (Transform _child in _obj.transform)
        {
            if (_child == null || LayerMask.LayerToName(_child.gameObject.layer) == "Collider")
                continue;

            SetLayerRecursively(_child.gameObject, _newLayer);
        }
    }

    public static string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

    public static PlayerWeapon[] AllWeapons()
    {
        Object[] allWeaponObjects = Resources.LoadAll("Prefabs/Weapons", typeof(GameObject));

        PlayerWeapon[] allWeapons = new PlayerWeapon[allWeaponObjects.Length];

        for (int i = 0; i < allWeaponObjects.Length; i++)
        {
            allWeapons[i] = ((GameObject)allWeaponObjects[i]).GetComponent<PlayerWeapon>();
        }

        return allWeapons;
    }
}