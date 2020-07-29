using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetItem : MonoBehaviour
{
    [SerializeField]
    private Text displayText = null;

    private SetList setList;

    public bool selected = false;

    public void Setup(Set _set, SetList _setList)
    {
        displayText.text = "<b>" + _set.name + ":</b> " + _set.description;
        setList = _setList;
    }
}
