using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnTargetCommand : ICommand
{
    public void Execute(PlayerBehaviour player)
    {
        switch (player.orbitCam.currentState)
        {
            case OrbitCamera.CamState.onPlayer:
                if (player.targetingSystem.SelectTarget(player.orbitCam))
                {
                    player.anim.SetBool("Target", true);
                    player.lockedOn = true;
                    player.lockOnFocus.gameObject.SetActive(true);
                    player.lockOnFocus.target = player.targetingSystem.currentTarget;
                }
                break;
            case OrbitCamera.CamState.onTarget:
                player.LoseTarget();
                break;
            default:
                break;
        }
    }
}
