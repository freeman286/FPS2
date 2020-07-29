﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KillfeedItem : MonoBehaviour
{

    [SerializeField]
    Text text = null;

    public void Setup(string player, string source)
    {
        text.text = "<b>" + source + "</b>" + " killed " + "<b>" + player + "</b>";
    }

}