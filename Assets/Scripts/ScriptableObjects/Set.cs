using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Set", menuName = "Sets/Set", order = 1)]
public class Set : ScriptableObject
{
    public new string name;
    public string description;

    [Header("Required items")]
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public Equipment equipment;

    [Header("Set damage attributes")]
    public List<DamageAttribute> attributes = new List<DamageAttribute>();

    [Header("Set movement attributes")]
    public float speedMultiplier = 1f;
    public float jumpMultiplier = 1f;

    [Header("Scripts To Enable")]
    public string[] scriptsToEnable;

}