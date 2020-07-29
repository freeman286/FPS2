using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetList : MonoBehaviour
{
    public static SetList instance;


    [SerializeField]
    private GameObject setItemPrefab = null;

    private Set[] allSets;

    public List<Set> activeSets;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one SetList in scene.");
        }
        else
        {
            instance = this;
        }
        allSets = SetsUtil.AllSets();
    }

    public void ShowSets()
    {
        Reset();
        SelectSets();
        foreach (Set _set in activeSets)
        {
            GameObject go = (GameObject)Instantiate(setItemPrefab, this.transform);
            SetItem item = go.GetComponent<SetItem>();
            item.Setup(_set, this);
        }
    }

    void SelectSets()
    {
        foreach (Set _set in allSets)
        {
            if (SetsUtil.SetMatch(_set, PlayerInfo.primaryWeaponName, PlayerInfo.secondaryWeaponName))
            {
                activeSets.Add(_set);
            }
        }
    }

    void Reset()
    {
        activeSets.Clear();

        foreach (Transform _child in transform)
        {
            Destroy(_child.gameObject);
        }
    }
    
}
