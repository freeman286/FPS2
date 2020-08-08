using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class MortarPistol : EnableDuringRuntime
{

    [SerializeField]
    private string detonateButton = "Fire2";

    private GameObject mortar;

    private ExplosiveController explosiveController;

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetButtonDown(detonateButton) && !Pause.IsOn)
        {
            mortar = GameManager.GetMortar();
            if (mortar != null && mortar.GetComponent<NetworkIdentity>().hasAuthority)
            {
                explosiveController = mortar.GetComponent<ExplosiveController>();
                explosiveController.Detonate();
            }
        }
    }
}
