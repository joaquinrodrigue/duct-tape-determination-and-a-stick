using System;
using UnityEngine;

public class PausedState : GameState
{
    public override void Enter()
    {
        gameStateController.PauseGame();
    }

    public override void Act()
    {
    }

    public override void CheckTransitions()
    {
        if (gameStateController.pauseActionRuntime.triggered)
        {
            gameStateController.ChangeState(gameStateController.playingState);
        }
    }

    public override void Exit()
    {
        gameStateController.ResumeGame();  
    }
}