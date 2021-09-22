using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCommand : ICommand
{
    public void Execute(PlayerBehaviour player)
    {
        player.anim.SetTrigger("Jump");
    }
}
