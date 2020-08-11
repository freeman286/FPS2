using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovementAbilityController : NetworkBehaviour
{
    [SerializeField]
    private ListType listType = null;

    private Behaviour script;

    void Start()
    {
        GetComponent<Player>().onPlayerSetDefaultsCallback += SetDefaults;
    }

    void SetDefaults()
    {
        if (isLocalPlayer)
        {
            if (script != null)
                script.enabled = false;

            ScriptID _scriptID = Util.NameToScriptID(PlayerInfo.GetNameSelected(listType));

            script = Util.EnableScipt(gameObject, _scriptID, true);
        }
    }
}
