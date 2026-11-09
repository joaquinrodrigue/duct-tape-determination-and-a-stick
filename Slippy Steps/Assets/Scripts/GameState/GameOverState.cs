using System;
using UnityEngine;

public class GameOverState : GameState
{
    public override void Enter()
    {
        Debug.Log("Entering Game Over State");
    }

    public override void Act()
    {
        Debug.Log("Acting in Game Over State");
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game Over State");
    }
}