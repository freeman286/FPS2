using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprint : PlayerMovementAbility
{

    public override void DoAbility()
    {
        motor.Move(Util.Flatten(transform.forward).normalized * Mathf.Pow(metrics.GetSpeed(), 0.2f) * ability.magnitude);
    }

}