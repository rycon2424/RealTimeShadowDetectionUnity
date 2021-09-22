using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintCommand : ICommand
{
    public void Execute(PlayerBehaviour player)
    {
        //Toggle sprinting
        player.anim.SetBool("Sprinting", !player.anim.GetBool("Sprinting"));
    }
}
