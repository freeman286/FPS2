using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableScriptOnParent : MonoBehaviour
{
    [SerializeField]
    private string ScriptName;

    void Start()
    {
        var comp = transform.root.gameObject.GetComponent(ScriptName);
        (comp as Behaviour).enabled = true;
    }

    void OnDestroy()
    {
        var comp = transform.root.gameObject.GetComponent(ScriptName);
        (comp as Behaviour).enabled = false;
    }
}
