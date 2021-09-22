using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpCommand : ICommand
{
    public void Execute(PlayerBehaviour pb)
    {
        if (pb.orbitCam.transform.localEulerAngles.y > 70 && pb.orbitCam.transform.localEulerAngles.y < 290)
        {
            pb.stateMachine.GoToState(pb, "InAir");
            pb.anim.SetTrigger("Jump");
            pb.AddRotation(new Vector3(0, pb.orbitCam.transform.localEulerAngles.y, 0));
        }
        else if (!pb.RayHit(pb.transform.position + pb.transform.forward * 0.75f + pb.transform.up * 1.25f, pb.transform.up, 1.75f, pb.everything) && !pb.RayHit(pb.transform.position + pb.transform.up * 1.5f, pb.transform.forward, 1f, pb.everything))
        {
            pb.mountingWall = true;
            pb.anim.SetTrigger("Jump");
        }
    }
}
