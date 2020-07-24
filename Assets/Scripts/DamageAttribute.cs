using UnityEngine;

[System.Serializable]
public class DamageAttribute
{
    public bool self;
    public DamageType damageType;
    public float multiplier = 1f;

    public virtual DamageAttribute Copy()
    {
        DamageAttribute copy = new DamageAttribute();
        copy.self = this.self;
        copy.damageType = this.damageType;
        copy.multiplier = this.multiplier;
        return copy;
    }

}
