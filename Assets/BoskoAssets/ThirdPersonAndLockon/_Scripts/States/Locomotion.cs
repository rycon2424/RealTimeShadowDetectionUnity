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
        pb.cc.enabled = true;
        pb.airtime = 0;
    }

    public override void OnStateExit(PlayerBehaviour pb)
    {

    }
    
    public override void StateUpdate(PlayerBehaviour pb)
    {
        pb.Grounded();
        if (!pb.lockedOn)
        {
            pb.RotateToCam();
        }
        if (Input.GetKeyDown(pb.pc.crouch))
        {
            pb.stateMachine.GoToState(pb, "Crouch");
        }
        GrabLedge(pb);
        Movement(pb);
        pb.ShadowDash();
        pb.CanTarget();
        if (pb.airtime > 0.75f)
        {
            pb.anim.SetTrigger("fall");
            pb.stateMachine.GoToState(pb, "InAir");
        }
    }
    
    void GrabLedge(PlayerBehaviour pb)
    {
        if (pb.grounded == false)
        {
            if (Input.GetKey(pb.pc.grab))
            {
                pb.LedgeInfo();
            }
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
        //Sprinting
        if (Input.GetKey(pb.pc.sprint))
        {
            pb.anim.SetBool("Sprinting", true);
        }
        else
        {
            pb.anim.SetBool("Sprinting", false);
        }
        //Jump
        if (Input.GetKeyDown(pb.pc.jump))
        {
            pb.anim.SetTrigger("Jump");
            pb.stateMachine.GoToState(pb, "InAir");
        }
    }
}
