using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class HexGameManager : MonoBehaviour, IGameManager
{
    [SerializeField] InterfaceReference<IGamePlayer> localPlayerRef;
    [SerializeField] InterfaceReference<IGamePlayer> aiPlayerRef;
    IGamePlayer localPlayer => localPlayerRef.Value;
    IGamePlayer aiPlayer => aiPlayerRef.Value;

    [Inject]
    IGameBoard gameBoard;

    [Inject]
    IStaticEvents staticEvents;

    int currentPlayerTurn;
    public List<IGamePlayer> players = new List<IGamePlayer>();

    bool isStarted = false;
    public bool IsStarted { get => isStarted; set => isStarted = value; }

    public int CurrentPlayerTurn
    {
        get => currentPlayerTurn;
        set
        {
            currentPlayerTurn = value;
            StartTurn();
        }
    }

    public void StartGame()
    {
        localPlayer.PlayerIndex = 0;
        aiPlayer.PlayerIndex = 1;
        players.Add(localPlayer);
        players.Add(aiPlayer);
        gameBoard.OnGameStart(VersusGameMode.HumanVsAI, true);
        CurrentPlayerTurn = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameBoard.Initialize();
    }

    public void StartTurn()
    {
        gameBoard.SelectStartHexTilesForPlayer(CurrentPlayerTurn);
        staticEvents.OnTurnStart.Invoke();
    }

    public void EndTurn()
    {
        CurrentPlayerTurn = (CurrentPlayerTurn + 1) % players.Count;
        staticEvents.OnTurnEnd?.Invoke();
        Debug.Log("End Turn next player turn is " + CurrentPlayerTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
