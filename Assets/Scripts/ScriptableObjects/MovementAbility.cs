using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementAbility", menuName = "MovementAbility", order = 1)]
public class MovementAbility : ScriptableObject
{
    public Sprite icon;
    public float cooldown;
    public float magnitude;
    public float effectTime;
    public int damage;
    public DamageType damageType;
}
