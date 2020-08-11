using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{

    [SerializeField]
    private Text nameText = null;

    private SelectList selectList;

    public string nameToSelect;

    public bool selected = false;

    public void Setup(string _name, SelectList _list)
    {
        nameToSelect = _name;
        nameText.text = _name;
        selectList = _list;
    }

    public void Deselect()
    {
        selected = false;
        GetComponent<Button>().interactable = true;
    }

    public void Select()
    {
        selected = true;
        GetComponent<Button>().interactable = false;
    }

    public void SelectName()
    {
        selectList.Select(nameToSelect);
    }

}
