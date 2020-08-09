using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : PlayerMovementAbility
{
    private Vector3 dashDirection;

    public override void DoAbility()
    {
        if (Util.Flatten(metrics.velocity).magnitude > 0.1f && metrics.IsGrounded())
        {
            dashDirection = Util.Flatten(metrics.velocity).normalized;
            timeSinceMovementAbilityUsed = 0f;
        }
    }

    public override void Update()
    {
        base.Update();

        if (timeSinceMovementAbilityUsed < ability.effectTime)
        {
            motor.Move(dashDirection * ability.magnitude);
        }
    }
}