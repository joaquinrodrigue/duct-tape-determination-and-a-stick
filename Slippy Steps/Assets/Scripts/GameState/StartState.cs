using System;
using UnityEngine;

public class StartState : GameState
{
    public override void Enter()
    {
        Debug.Log("Entering Start State");
    }

    public override void Act()
    {
        Debug.Log("Acting in Start State");
    }

    public override void Exit()
    {
        Debug.Log("Exiting Start State");
    }
}