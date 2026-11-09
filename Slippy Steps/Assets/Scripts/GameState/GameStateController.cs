using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StartState))]
[RequireComponent(typeof(PausedState))]
[RequireComponent(typeof(PlayingState))]
[RequireComponent(typeof(GameOverState))]
public class GameStateController : MonoBehaviour
{
    [SerializeField] private GameState currentState;
    [SerializeField] private int timer;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private RubberbandController rubberbandController;
    [SerializeField] private Transform startPoint;
    [SerializeField] private GameObject playerPrefab;

    private bool isPaused;
    private List<GameObject> players = new();
    private List<InputDevice> inputDevices = new();

    public void ChangeState(GameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.gameStateController = this;
        currentState.Enter();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetGame();
    }

    private void UpdateTimer()
    {
        if(!isPaused)
        {
            String time = string.Format("{0:D2}:{1:D2}", timer / 60, timer % 60);
            timerText.text = time;
            timer--;
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void ResetGame()
    {
        ResetState();
        ResetTimer();
        ResetInputDevices();
        ResetPlayers();
    }

    private void ResetState()
    {
        currentState = new StartState { gameStateController = this };
        currentState.Enter();
    }
    
    private void ResetTimer()
    {
        CancelInvoke(nameof(UpdateTimer));
        timer = 450;
        InvokeRepeating(nameof(UpdateTimer), 1f, 1f);
    }

    private void ResetInputDevices()
    {
        inputDevices.Clear();
        foreach (var device in InputSystem.devices)
        {
            if(device is Gamepad || device is ArduinoControlDevice)
            {
                inputDevices.Add(device);
            }
        }
    }

    private void ResetPlayers()
    {
        foreach (var player in players)
        {
            Destroy(player);
        }
        players.Clear();
        rubberbandController.transform.position = startPoint.position;
        for (int i = 0; i < inputDevices.Count; i++)
        {
            PlayerInput playerInput = PlayerInput.Instantiate(playerPrefab, pairWithDevice: inputDevices[i]);
            players.Add(playerInput.gameObject);
            players[^1].transform.position = startPoint.position + 3 * Mathf.Sin(i * Mathf.PI / 2) * Vector3.right + 3 * Mathf.Cos(i * Mathf.PI / 2) * Vector3.forward;
            rubberbandController.AddPlayer(players[^1].GetComponent<Rigidbody>());
        }
    }
}
