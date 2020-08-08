using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprint : PlayerMovementAbility
{

    public override void DoAbility()
    {
        motor.Move(Util.Flatten(transform.forward).normalized * ability.magnitude);
    }

}