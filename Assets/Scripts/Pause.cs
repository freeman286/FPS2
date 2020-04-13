using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Pause : MonoBehaviour
{

    public static bool IsOn = false;

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.singleton;
    }

    public void Disconnect()
    {
        IsOn = false;
        networkManager.StopClient();
        networkManager.StopHost();
    }

}
