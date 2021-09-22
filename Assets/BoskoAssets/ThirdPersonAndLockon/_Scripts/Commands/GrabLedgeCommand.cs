using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabLedgeCommand : ICommand
{
    public void Execute(PlayerBehaviour player)
    {
        if (player.grounded == false)
        {
            player.LedgeInfo();
        }
    }
}
