using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Pause : MonoBehaviour
{

    [SerializeField]
    private PlayerUI ui;

    public static bool IsOn = false;

    public void Disconnect()
    {
        LevelLoader.instance.DoTransition();
        IsOn = false;
        ui.Disconnect();
    }

}
