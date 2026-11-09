using System;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GameState
{
    public GameStateController gameStateController;

    public abstract void Enter();

    public abstract void Act();

    public abstract void Exit();

}