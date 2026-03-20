using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
        Ctx.Animator.SetBool(Ctx.IsWalkingHash, true);
        Ctx.Animator.SetBool(Ctx.IsRunningHash, true);
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        Vector3 targetDirection = Quaternion.Euler(0, Ctx.TargetRotation, 0) * Vector3.forward;
        Ctx.AppliedMovementX = targetDirection.x * 2.2f * Ctx.SpeedMultiplier;
        Ctx.AppliedMovementZ = targetDirection.z * 2.2f * Ctx.SpeedMultiplier;
    }
    public override void ExitState()
    {
    }

    public override void InitializeSubState()
    {
    }
    public override void CheckSwitchStates()
    {
        if (!Ctx.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
        else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
        {
            SwitchState(Factory.Walk());
        }
    }
}
