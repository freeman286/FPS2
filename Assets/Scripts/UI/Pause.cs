using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{

    public static Pause instance;

    [SerializeField]
    private PlayerUI ui = null;

    public static bool isOn = false;

    public delegate void PausedCallback();
    public PausedCallback pausedCallback;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one Pause in scene.");
        }
        else
        {
            instance = this;
        }
    }

    public void Disconnect()
    {
        LevelLoader.instance.DoTransition();
        isOn = false;
        ui.Disconnect();
    }

}
