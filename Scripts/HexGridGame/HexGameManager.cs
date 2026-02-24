using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class HexGameManager : MonoBehaviour, IGameManager
{
    [SerializeField] InterfaceReference<IGamePlayer> localPlayerRef;
    [SerializeField] InterfaceReference<IGamePlayer> aiPlayerRef;
    [SerializeField] VersusGameMode startingMode = VersusGameMode.HumanVsAI;
    IGamePlayer localPlayer => localPlayerRef.Value;
    IGamePlayer aiPlayer => aiPlayerRef.Value;

    [Inject]
    IGameBoard gameBoard;

    [Inject]
    IStaticEvents staticEvents;

    TurnPhase currentPhase = TurnPhase.Placement;
    IGameModeRules gameModeRules;

    int currentPlayerTurn;
    List<IGamePlayer> players = new List<IGamePlayer>();

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

    public List<IGamePlayer> Players { get => players; set => players = value; }

    public void StartGame()
    {
        localPlayer.PlayerIndex = 0;
        aiPlayer.PlayerIndex = 1;
        players.Add(localPlayer);
        players.Add(aiPlayer);
        gameBoard.OnGameStart(gameModeRules.Mode, true);
        CurrentPlayerTurn = gameModeRules.GetStartingPlayerIndex(players);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gameModeRules = GameModeRulesFactory.Create(startingMode);
    }

    void Start()
    {
        gameBoard.Initialize();
        staticEvents.OnCardPlaced += HandleCardPlaced;
        staticEvents.OnActionResolved += HandleActionResolved;
    }

    private void OnDestroy()
    {
        staticEvents.OnCardPlaced -= HandleCardPlaced;
        staticEvents.OnActionResolved -= HandleActionResolved;
    }

    public void StartTurn()
    {
        currentPhase = TurnPhase.Placement;
        gameBoard.SelectStartHexTilesForPlayer(CurrentPlayerTurn);
        staticEvents.OnTurnStart?.Invoke();
    }

    public void EndTurn()
    {
        CurrentPlayerTurn = gameModeRules.GetNextPlayerIndex(CurrentPlayerTurn, players);
        staticEvents.OnTurnEnd?.Invoke();
        Debug.Log("End Turn next player turn is " + CurrentPlayerTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<IGamePlayer> GetPlayers()
    {
        return players;
    }

    public void ResolveActionForCurrentPlayer()
    {
        staticEvents.OnActionResolved?.Invoke(players[CurrentPlayerTurn]);
    }

    void HandleCardPlaced(IGamePlayer actingPlayer)
    {
        if (!IsCurrentPlayer(actingPlayer))
        {
            return;
        }

        if (gameModeRules.CanEnterActionPhase(actingPlayer))
        {
            currentPhase = TurnPhase.Action;
            Debug.Log($"Action phase started for player {CurrentPlayerTurn}");
            if (gameModeRules.AutoResolveActions(actingPlayer))
            {
                ResolveActionForCurrentPlayer();
            }
            else if (!HasActionableTargets(actingPlayer))
            {
                // When there is nothing to target, resolve the action phase to keep turns flowing.
                ResolveActionForCurrentPlayer();
            }
        }
        else
        {
            EndTurn();
        }
    }

    void HandleActionResolved(IGamePlayer actingPlayer)
    {
        if (!IsCurrentPlayer(actingPlayer))
        {
            return;
        }
        EndTurn();
    }

    bool IsCurrentPlayer(IGamePlayer player)
    {
        return CurrentPlayerTurn < players.Count && players[CurrentPlayerTurn] == player;
    }

    bool HasActionableTargets(IGamePlayer actingPlayer)
    {
        if (gameBoard?.GridPlaceables == null)
        {
            return false;
        }
        foreach (var placeable in gameBoard.GridPlaceables.Values)
        {
            if (placeable != null && placeable.player != null && placeable.player != actingPlayer)
            {
                return true;
            }
        }
        return false;
    }
}
