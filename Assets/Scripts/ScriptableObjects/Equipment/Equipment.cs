using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Equipment : ScriptableObject {
    public float cooldown;
    public float range;
    public GameObject prefab;
}

