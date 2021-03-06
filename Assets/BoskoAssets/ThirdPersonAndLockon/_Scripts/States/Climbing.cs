using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : State
{
    Vector3 defaultCenter = new Vector3(0,1,0);
    float defaultHeight = 1.75f;

    public override void OnStateEnter(PlayerBehaviour pb)
    {
        pb.playerControls.UnBindInput(KeyCode.Space);
        pb.playerControls.BindInputToCommand(KeyCode.Space, new WallJumpCommand(), KeyCommand.KeyType.OnKeyDown);

        pb.mountingWall = false;
        climbCooldown = true;
        pb.mono.StartCoroutine(ClimbCooldown(pb.anim, 0.3f, null));

        pb.anim.applyRootMotion = false;
        pb.characterController.enabled = false;

        pb.RayHit(pb.transform.position + (pb.transform.forward * 0.6f) + (pb.transform.up * 1.7f), Vector3.down, 0.35f, pb.everything);

        Vector3 newPlayerPos = pb.transform.position;
        newPlayerPos.y = pb.lastCachedhit.y - pb.grabHeight;
        
        pb.transform.position = newPlayerPos;

        if (!pb.PlayerFaceWall(pb, new Vector3(0, 1, 0), pb.transform.forward, 2))
        {
            pb.stateMachine.GoToState(pb, "Locomotion");
            return;
        }
        if (!pb.PlayerToWall(pb, pb.transform.forward, false, 1.2f))
        {
            pb.stateMachine.GoToState(pb, "Locomotion");
            return;
        }

        Vector3 ccCenter= new Vector3(0, 0.6f, 0);
        pb.characterController.center = ccCenter;
        pb.characterController.height = 1.2f;

        pb.anim.SetTrigger("Climb");
        pb.DelayFunction("DelayedRoot", 0.25f);
        
    }

    public override void OnStateExit(PlayerBehaviour pb)
    {
        pb.playerControls.UnBindInput(KeyCode.Space);
        pb.playerControls.BindInputToCommand(KeyCode.Space, new JumpCommand(), KeyCommand.KeyType.OnKeyDown);

        pb.characterController.center = defaultCenter;
        pb.characterController.height = defaultHeight;
        pb.characterController.enabled = true;
        pb.airtime = 0;
    }
    
    private bool climbCooldown;
    public override void StateUpdate(PlayerBehaviour pb)
    {
        pb.anim.SetFloat("LookDirection", pb.orbitCam.transform.localEulerAngles.y);
        
        if (pb.mountingWall == true || climbCooldown == true)
        {
            return;
        }

        ShimmyClimbing(pb);

        ClimbingUpDown(pb);
    }

    IEnumerator ClimbCooldown(Animator anim, float cld, PlayerBehaviour pb)
    {
        yield return new WaitForSeconds(cld);
        if (pb != null)
        {
            pb.PlayerToWall(pb, pb.transform.forward, false, 1.2f);
        }
        climbCooldown = false;
    }

    void ClimbingUpDown(PlayerBehaviour pb)
    {
        int climbDirection = (int)Input.GetAxisRaw(pb.playerControls.inputVertical);
        if (climbDirection != 0)
        {
            if (climbDirection == -1)
            {
                if (pb.RayHit(pb.transform.position + pb.transform.forward * 0.3f + pb.transform.up + pb.transform.right * 0.2f, Vector3.down, 1.5f, pb.everything) &&
                    pb.RayHit(pb.transform.position + pb.transform.forward * 0.3f + pb.transform.up + -pb.transform.right * 0.1f, Vector3.down, 1.5f, pb.everything))
                {
                    Vector3 temp = pb.lastCachedhit;
                    temp.x = pb.transform.position.x;
                    temp.z = pb.transform.position.z;
                    temp.y -= pb.grabHeight;

                    pb.LerpToPosition(temp, 0.35f);
                    pb.anim.SetTrigger("JumpClimb");
                    pb.anim.SetInteger("ClimbUpDirection", -1);
                    climbCooldown = true;
                    pb.mono.StartCoroutine(ClimbCooldown(pb.anim, 0.7f, pb));
                }
            }
            else if (climbDirection == 1)
            {
                if (pb.RayHit(pb.transform.position + pb.transform.forward * 0.3f + pb.transform.up * 3f + pb.transform.right * 0.2f, Vector3.down, 1.5f, pb.everything) &&
                    pb.RayHit(pb.transform.position + pb.transform.forward * 0.3f + pb.transform.up * 3f + -pb.transform.right * 0.1f, Vector3.down, 1.5f, pb.everything))
                {
                    Vector3 temp = pb.lastCachedhit;
                    temp.x = pb.transform.position.x;
                    temp.z = pb.transform.position.z;
                    temp.y -= pb.grabHeight;

                    pb.LerpToPosition(temp, 0.35f);
                    pb.anim.SetTrigger("JumpClimb");
                    pb.anim.SetInteger("ClimbUpDirection", 1);
                    climbCooldown = true;
                    pb.mono.StartCoroutine(ClimbCooldown(pb.anim, 0.7f, pb));
                }
                pb.anim.SetFloat("ClimbingHand", pb.anim.GetFloat("ClimbingHand") * -1);
            }
        }
        else
        {
            pb.anim.SetInteger("ClimbUpDirection", 0);
        }
    }

    void ShimmyClimbing(PlayerBehaviour pb)
    {
        int shimmyDirection = (int)Input.GetAxisRaw(pb.playerControls.inputHorizontal);
        if (shimmyDirection != 0)
        {
            pb.PlayerToWall(pb, pb.transform.forward, false, 1.2f);
            if (shimmyDirection == 1)
            {
                if (
                    !pb.RayHit(pb.transform.position + pb.transform.up * 0.5f, pb.transform.right, 0.65f, pb.everything) &&
                     pb.RayHit(pb.transform.position + pb.transform.right * 0.6f + pb.transform.up * 1.2f, pb.transform.forward, 0.35f, pb.everything) &&
                    !pb.RayHit(pb.transform.position + pb.transform.forward * 0.4f + pb.transform.up * 1.4f, pb.transform.right, 0.5f, pb.everything)
                   )
                {
                    pb.anim.SetInteger("ClimbDirection", shimmyDirection);
                }
                else
                {
                    pb.anim.SetInteger("ClimbDirection", 0);
                }
            }
            else if (shimmyDirection == -1)
            {
                if (
                    !pb.RayHit(pb.transform.position + pb.transform.up * 0.5f, pb.transform.right, -0.65f, pb.everything) &&
                     pb.RayHit(pb.transform.position + pb.transform.right * -0.6f + pb.transform.up * 1.2f, pb.transform.forward, 0.35f, pb.everything) &&
                    !pb.RayHit(pb.transform.position + pb.transform.forward * 0.4f + pb.transform.up * 1.4f, -pb.transform.right, 0.5f, pb.everything)
                   )
                {
                    pb.anim.SetInteger("ClimbDirection", shimmyDirection);
                }
                else
                {
                    pb.anim.SetInteger("ClimbDirection", 0);
                }
            }
        }
        else
        {
            pb.anim.SetInteger("ClimbDirection", 0);
        }
    }
}
