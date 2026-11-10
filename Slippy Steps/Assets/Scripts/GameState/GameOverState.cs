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
        
    }

    public override void CheckTransitions()
    {
        if (gameStateController.restartActionRuntime.triggered)
        {
            gameStateController.ChangeState(gameStateController.startState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game Over State");
    }
}