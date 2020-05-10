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

    public static Vector2 SnapTo(Vector2 _vector)
    {
        Vector2[] basis = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        for (int i = 0; i < basis.Length; i++)
        {
            if (Mathf.Abs(Vector2.Angle(_vector, basis[i])) <= 45)
            {
                return basis[i];
            }
        }

        return basis[0];
    }

    public static int[,,] TransposeMatrix(int[,,] _matrix)
    {
        int x = _matrix.GetLength(0);
        int y = _matrix.GetLength(1);
        int z = _matrix.GetLength(2);

        int[,,] result = new int[x, y, z];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    result[j, i, k] = _matrix[i, j, k];
                }
            }
        }

        return result;
    }

    public static int[,,] FlipMatrix(int[,,] _matrix)
    {
        int x = _matrix.GetLength(0);
        int y = _matrix.GetLength(1);
        int z = _matrix.GetLength(2);

        int[,,] result = new int[x, y, z];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    result[i, j, k] = _matrix[(x - 1) - i, j, k];
                }
            }
        }

        return result;
    }

    public static int[,,] RotateMatrix(int[,,] _matrix)
    {
        return FlipMatrix(TransposeMatrix(_matrix));
    }

    public static string FormatMatrix(int[,,] _matrix)
    {
        string result = "";
        for (int d0 = 0; d0 < _matrix.GetLength(0); d0++)
        {
            if (d0 > 0) result += ",";
            result += "[";
            for (int d1 = 0; d1 < _matrix.GetLength(1); d1++)
            {
                if (d1 > 0) result += ",";
                result += "[";
                for (int d2 = 0; d2 < _matrix.GetLength(2); d2++)
                {
                    if (d2 > 0) result += ",";
                    result += _matrix[d0, d1, d2].ToString();
                }
                result += "]";
            }
            result += "]";
        }
        result += "]";
        return result;
    }
}