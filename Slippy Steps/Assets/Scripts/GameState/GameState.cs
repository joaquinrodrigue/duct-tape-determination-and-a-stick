using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GameState : MonoBehaviour
{
    protected InputActionAsset inputActionAsset;

    public GameStateController gameStateController;

    public abstract void Enter();

    public abstract void Act();

    public abstract void CheckTransitions();

    public abstract void Exit();

    void FixedUpdate()
    {
        if (gameStateController != null && gameStateController.currentState == this)
        {
            Act();
            CheckTransitions();
        }
    }

}