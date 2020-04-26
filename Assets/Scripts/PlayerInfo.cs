﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance;

    static public string playerName;

    static public string primaryWeaponName;

    static public string secondaryWeaponName;

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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(primaryWeaponName);
            Debug.Log(secondaryWeaponName);
        }
    }

}
