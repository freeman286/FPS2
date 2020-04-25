using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Pause : MonoBehaviour
{

    [SerializeField]
    private PlayerUI ui;

    public static bool IsOn = false;

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = ui.networkManager;
    }

    public void Disconnect()
    {
        IsOn = false;
        networkManager.StopClient();
        networkManager.StopHost();
    }

}
