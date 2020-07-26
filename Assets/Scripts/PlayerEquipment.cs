using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
public class PlayerEquipment : NetworkBehaviour
{
    private Equipment equipment;

    void Start()
    {
        SetDefaults();
    }

    void Update()
    {
        if (isLocalPlayer && Input.GetButtonDown("Equipment")) {
            if (equipment is Grenade)
            {
                //Do something
            }
        }
    }

    public void SetDefaults()
    {
        if (isLocalPlayer)
        {
            equipment = EquipmentUtil.NameToEquipment(PlayerInfo.equipmentName);
        }
    }
}
