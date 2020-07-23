﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killfeed : MonoBehaviour
{

    [SerializeField]
    private GameObject killfeedItemPrefab = null;

    // Use this for initialization
    void Start()
    {
        GameManager.instance.onPlayerKilledCallback += OnKill;
    }

    public void OnKill(string player, string source)
    {
        GameObject go = (GameObject)Instantiate(killfeedItemPrefab, this.transform);
        go.GetComponent<KillfeedItem>().Setup(player, source);
        go.transform.SetAsFirstSibling();

        Destroy(go, 8f);
    }

}
