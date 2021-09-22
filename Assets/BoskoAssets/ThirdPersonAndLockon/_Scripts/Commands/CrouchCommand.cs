using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchCommand : ICommand
{
    public void Execute(PlayerBehaviour player)
    {
        switch (player.stateMachine.CurrentState().ToString())
        {
            case "Locomotion":
                player.stateMachine.GoToState(player, "Crouch");
                break;
            case "Crouch":
                player.stateMachine.GoToState(player, "Locomotion");
                break;
            case "Climbing":
                player.anim.SetTrigger("LetGo");
                player.stateMachine.GoToState(player, "InAir");
                break;
            default:
                Debug.LogWarning("Called crouchCommand from a state that is not defined here");
                break;
        }
    }
}
