using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StartState))]
[RequireComponent(typeof(PausedState))]
[RequireComponent(typeof(PlayingState))]
[RequireComponent(typeof(GameOverState))]
public class GameStateController : MonoBehaviour
{
    [SerializeField] private int timer;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private RubberbandController rubberbandController;
    [SerializeField] private Transform startPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private InputActionAsset inputActionAsset;

    private bool isPaused;
    private List<GameObject> players = new();
    private List<InputDevice> inputDevices = new();

    public InputActionReference pauseAction;
    public InputActionReference restartAction;
    public InputAction pauseActionRuntime;
    public InputAction restartActionRuntime;

    public StartState startState;
    public PausedState pausedState;
    public PlayingState playingState;
    public GameOverState gameOverState;
    public GameState currentState;

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
        pauseActionRuntime = inputActionAsset.FindAction(pauseAction.action.name);
        restartActionRuntime = inputActionAsset.FindAction(restartAction.action.name);
        pauseActionRuntime.Enable();
        restartActionRuntime.Enable();

        startState = GetComponent<StartState>();
        pausedState = GetComponent<PausedState>();
        playingState = GetComponent<PlayingState>();
        gameOverState = GetComponent<GameOverState>();
        ChangeState(startState);
    }

    private void UpdateTimer()
    {
        string time = string.Format("{0:D2}:{1:D2}", timer / 60, timer % 60);
        timerText.text = time;
        timer--;
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
        ResetTimer();
        ResetInputDevices();
        StartCoroutine(ResetPlayers());
    }
    
    private void ResetTimer()
    {
        CancelInvoke(nameof(UpdateTimer));
        timer = 450;
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

    private IEnumerator ResetPlayers()
    {
        foreach (var player in players)
        {
            Destroy(player);
        }
        players.Clear();

        yield return new WaitUntil(() => players.Count == 0);

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
