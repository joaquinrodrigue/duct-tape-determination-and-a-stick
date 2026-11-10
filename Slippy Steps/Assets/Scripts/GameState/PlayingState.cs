using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayingState : GameState
{
    public override void Enter()
    {
        Debug.Log("Entering Playing State");
        
        gameStateController.InvokeRepeating(nameof(UpdateTimer), 1f, 1f);
    }

    private void UpdateTimer()
    {
        gameStateController.SendMessage("UpdateTimer");
    }

    public override void Act()
    {
        CheckTransitions();
    }

    public override void CheckTransitions()
    {
        if (gameStateController.pauseActionRuntime.triggered)
        {
            gameStateController.ChangeState(gameStateController.pausedState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Playing State");
        gameStateController.CancelInvoke(nameof(UpdateTimer));
    }
}
