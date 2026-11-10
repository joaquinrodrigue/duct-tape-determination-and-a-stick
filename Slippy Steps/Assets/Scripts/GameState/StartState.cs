using System;
using UnityEngine;

public class StartState : GameState
{
    public override void Enter()
    {
        gameStateController.ResetGame();
    }

    public override void Act()
    {
        
    }

    public override void CheckTransitions()
    {
        gameStateController.ChangeState(gameStateController.playingState);
    }

    public override void Exit()
    {
    }
}