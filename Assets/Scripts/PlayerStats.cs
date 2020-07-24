using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    private WeaponManager weaponManager;

    private Set[] allSets;

    public List<Set> activeSets;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        allSets = SetsUtil.AllSets();
    }

    public void GetSets()
    {
        if (allSets == null)
            return; // Things haven't loaded properly yet

        activeSets.Clear();

        foreach (Set _set in allSets)
        {
            if (SetsUtil.SetMatch(_set, weaponManager))
            {
                activeSets.Add(_set);
            }
        }
    }
}
