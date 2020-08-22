using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KillStreak", menuName = "KillStreak", order = 1)]
public class KillStreak : ScriptableObject
{
    public Sprite icon;
    public int kills;
    public float time;
    public GameObject prefab;
}
