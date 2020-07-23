using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableScriptOnParent : MonoBehaviour
{
    [SerializeField]
    private string ScriptName = null;

    void Start()
    {
        EnableScipt(true);
    }

    void OnDestroy()
    {
        EnableScipt(false);
    }

    void EnableScipt(bool _enable)
    {
        var comp = transform.root.gameObject.GetComponent(ScriptName);
        (comp as Behaviour).enabled = _enable;
    }
}
