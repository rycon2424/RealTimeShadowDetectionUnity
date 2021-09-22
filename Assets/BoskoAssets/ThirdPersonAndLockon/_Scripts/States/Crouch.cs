using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : State
{
    public override void OnStateEnter(PlayerBehaviour pb)
    {
        pb.anim.SetBool("Crouch", true);
    }

    public override void OnStateExit(PlayerBehaviour pb)
    {
        pb.anim.SetBool("Crouch", false);
    }

    public override void StateUpdate(PlayerBehaviour pb)
    {
        pb.Grounded();
        pb.RotateToCam();
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
