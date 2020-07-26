using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Equipment : ScriptableObject {
    public new string name;
    public float cooldown;
}

