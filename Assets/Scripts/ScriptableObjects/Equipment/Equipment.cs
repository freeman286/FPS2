using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment : ScriptableObject {
    public Sprite icon;
    public float cooldown;
    public float range;
    public GameObject prefab;
}

