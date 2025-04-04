using Reflex.Attributes;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IGameManager
{
    public bool IsStarted { get; set; }
    public void StartGame();
    public int CurrentPlayerTurn { get; set; }
    public void EndTurn();
    public void StartTurn();
    public List<IGamePlayer> Players { get; set; }

}

public class NetworkGameManager : NetworkBehaviour, IGameManager
{
    [Inject]
    IGameBoard gameBoard;
    private NetworkVariable<int> currentPlayerTurn = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>(false);

    private int totalPlayers;
    bool IsOnline => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    bool RunLocal => !IsOnline || IsServer;

    bool isStarted = false;
    public bool IsStarted { get => isStarted; set => isStarted = value; }

    public int CurrentPlayerTurn { get => currentPlayerTurn.Value; set => currentPlayerTurn.Value = value; }
    public List<IGamePlayer> Players { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    //public GameObject offlineObjects;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        }
        //offlineObjects?.SetActive(false);
        SetGameBoard();
    }

    public void StartGame()
    {
        if (!IsOnline)
        {
            totalPlayers = 2;
            gameBoard.SelectStartHexTilesForPlayer(0);
            gameBoard.OnGameStart(VersusGameMode.HumanVsHuman, IsOnline);
            Debug.Log("Game started OFFLINE");
        }
        else
            StartGameRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        if (!isGameActive.Value) return;

        currentPlayerTurn.Value = (currentPlayerTurn.Value + 1) % totalPlayers;
        NotifyPlayerTurnClientRpc(currentPlayerTurn.Value);
    }

    [ClientRpc]
    private void NotifyPlayerTurnClientRpc(int playerIndex)
    {
        if (NetworkManager.Singleton.LocalClientId == (ulong)playerIndex)
        {
            Debug.Log("It's your turn!");
            EnablePlayerControls();
            gameBoard.SelectStartHexTilesForPlayer(playerIndex);
        }
        else
        {
            Debug.Log($"Waiting for Player {playerIndex} to finish their turn.");
            DisablePlayerControls();
        }
    }

    private void EnablePlayerControls()
    {
        // Enable controls specific to the local player
        // For example, allow card interactions or board placements
    }

    private void DisablePlayerControls()
    {
        // Disable controls to prevent unauthorized actions
    }

    public void CheckGameOver()
    {
        if (IsServer)
        {
            // Add your game-over logic here (e.g., check win conditions)
            bool isGameOver = false; // Replace with actual conditions

            if (isGameOver)
            {
                isGameActive.Value = false;
                EndGameClientRpc();
            }
        }
    }

    private void OnEnable()
    {
        currentPlayerTurn.OnValueChanged += OnTurnChanged;
    }

    private void OnDisable()
    {
        currentPlayerTurn.OnValueChanged -= OnTurnChanged;
    }

    private void OnTurnChanged(int oldValue, int newValue)
    {
        Debug.Log($"Turn changed from Player {oldValue} to Player {newValue}");
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        Debug.Log("Game Over!");
        // Show end-game UI or handle post-game logic
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void StartGameRpc()
    {
        if (RunLocal)
        {
            isGameActive.Value = true;
            currentPlayerTurn.Value = 0;
            NotifyPlayerTurnClientRpc(currentPlayerTurn.Value);
        }
        gameBoard.OnGameStart(VersusGameMode.HumanVsHuman, IsOnline);
    }

   

    public void SetGameBoard()
    {
        if (IsServer)
        {
            gameBoard.Initialize();
            GeneratedBoardData boardData = gameBoard.boardData;
            Debug.Log("Setting board data for clients board data count: " + boardData.boardTiles.Length);
            SetBoardDataClientRpc(boardData);
        }
        else
            SetGameBoardRpc();
    }

    [Rpc(SendTo.Server)]
    public void SetGameBoardRpc()
    {
        SetGameBoard();
    }

    [Rpc(SendTo.NotMe)]
    public void SetBoardDataClientRpc(GeneratedBoardData boardData)
    {
        gameBoard.boardData = boardData;
        gameBoard.Initialize();
    }

    public void OnPlayerConnected()
    {
        Debug.Log("Player connected");
    }

    public void EndTurn()
    {
        throw new System.NotImplementedException();
    }

    public void StartTurn()
    {
        throw new System.NotImplementedException();
    }
}
