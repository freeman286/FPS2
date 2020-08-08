using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementAbility", menuName = "MovementAbility", order = 1)]
public class MovementAbility : ScriptableObject
{
    public new string name;
    public float cooldown;
    public float magnitude;
    public float effectTime;
}
