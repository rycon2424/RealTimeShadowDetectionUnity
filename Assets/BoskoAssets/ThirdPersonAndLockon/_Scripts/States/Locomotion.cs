using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : State
{

    public override void AnimatorIKUpdate(PlayerBehaviour pb)
    {

    }

    public override void OnStateEnter(PlayerBehaviour pb)
    {
        pb.anim.ResetTrigger("Jump");
        pb.anim.applyRootMotion = true;
        pb.characterController.enabled = true;
        pb.airtime = 0;
        pb.playerControls.BindInputToCommand(KeyCode.F, new LockOnTargetCommand(), KeyCommand.KeyType.OnKeyDown);
    }

    public override void OnStateExit(PlayerBehaviour pb)
    {
        pb.playerControls.UnBindInput(KeyCode.F);
    }
    
    public override void StateUpdate(PlayerBehaviour pb)
    {
        pb.Grounded();
        if (!pb.lockedOn)
        {
            pb.RotateToCam();
        }
        Movement(pb);
        pb.ShadowDash();
        if (pb.airtime > 0.75f)
        {
            pb.anim.SetTrigger("fall");
            pb.stateMachine.GoToState(pb, "InAir");
        }
    }
    
    void Movement(PlayerBehaviour pb)
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        pb.anim.SetFloat("x", x);
        pb.anim.SetFloat("y", y);
        pb.anim.SetFloat("y+x", (Mathf.Abs(x) + Mathf.Abs(y)));

        //Walking
        if (x != 0 || y != 0)
        {
            pb.anim.SetBool("Walking", true);
        }
        else
        {
            pb.anim.SetBool("Walking", false);
        }
    }
}
