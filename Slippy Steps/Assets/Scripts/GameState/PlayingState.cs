using System;
using UnityEngine;

public class PlayingState : GameState
{
    public override void Enter()
    {
        Debug.Log("Entering Playing State");
    }

    public override void Act()
    {
        Debug.Log("Acting in Playing State");
    }

    public override void Exit()
    {
        Debug.Log("Exiting Playing State");
    }
}
