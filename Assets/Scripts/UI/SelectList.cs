using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class SelectList : MonoBehaviour
{
    [SerializeField]
    private ListType listType;

    [SerializeField]
    private GameObject UIItemPrefab = null;

    private string[] allElements;

    private UIItem[] items;

    public string selectedName;

    void Start()
    {
        allElements = Util.GetNamesInDirectory(listType.path);
        items = new UIItem[allElements.Length];

        for (int i = 0; i < allElements.Length; i++)
        {
            GameObject go = (GameObject)Instantiate(UIItemPrefab, this.transform);
            UIItem item = go.GetComponent<UIItem>();
            item.Setup(allElements[i], this);
            items[i] = item;
        }

        if (string.IsNullOrEmpty(PlayerInfo.GetNameSelected(listType)))
        {
            PlayerInfo.SelectName(listType, allElements[0]);
        }

        Select(PlayerInfo.GetNameSelected(listType));
    }

    public void Select(string _name)
    {
        PlayerInfo.SelectName(listType, _name);

        for (int i = 0; i < items.Length; i++)
        {
            if (_name == items[i].nameToSelect)
            {
                items[i].Select();
            }
            else
            {
                items[i].Deselect();
            }
        }

        SetList.instance.ShowSets();
    }
}

