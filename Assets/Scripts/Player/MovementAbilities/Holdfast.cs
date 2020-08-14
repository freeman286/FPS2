using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holdfast : PlayerMovementAbility
{
    public override void DoAbility()
    {
        if (metrics.IsGrounded())
        {
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }
    }

    public override void ExitAbility()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}