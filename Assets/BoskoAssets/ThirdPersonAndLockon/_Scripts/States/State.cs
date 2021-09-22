using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public virtual void OnStateEnter(PlayerBehaviour pb)
    {
    }

    public virtual void OnStateExit(PlayerBehaviour pb)
    {
    }

    public virtual void StateUpdate(PlayerBehaviour pb)
    {
    }

    public virtual void StateLateUpdate(PlayerBehaviour pb)
    {
    }

    public virtual void StateFixedUpdate(PlayerBehaviour pb)
    {
    }

    public virtual void AnimatorIKUpdate(PlayerBehaviour pb)
    {
    }

}
