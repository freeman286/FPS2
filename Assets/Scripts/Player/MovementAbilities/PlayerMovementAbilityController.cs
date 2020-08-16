using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovementAbilityController : NetworkBehaviour
{
    [SerializeField]
    private ListType listType = null;

    private PlayerMovementAbility movementAbility;

    public delegate void OnMovementAbilityChangedCallback(Sprite icon);
    public OnMovementAbilityChangedCallback onMovementAbilityChangedCallback;

    public float GetAbilityPct()
    {
        if (movementAbility == null)
            return 1;

        return movementAbility.GetAbilityPct();
    }

    void Start()
    {
        GetComponent<Player>().onPlayerSetDefaultsCallback += SetDefaults;
    }

    void SetDefaults()
    {
        if (isLocalPlayer)
        {
            if (movementAbility != null)
                movementAbility.enabled = false;

            ScriptID _scriptID = Util.NameToScriptID(PlayerInfo.GetNameSelected(listType));

            movementAbility = Util.EnableScipt(gameObject, _scriptID, true) as PlayerMovementAbility;

            onMovementAbilityChangedCallback.Invoke(movementAbility.ability.icon);
        }
    }
}
