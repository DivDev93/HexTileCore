using Reflex.Attributes;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [Inject]
    HexGridManager hexGridManager;
    private NetworkVariable<int> currentPlayerTurn = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>(false);

    private int totalPlayers;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        }           
        SetGameBoard();
    }

    public void StartGame()
    {
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
            hexGridManager.SelectStartHexTilesForPlayer(playerIndex);
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
        if (IsServer)
        {
            isGameActive.Value = true;
            currentPlayerTurn.Value = 0;
            NotifyPlayerTurnClientRpc(currentPlayerTurn.Value);
        }
        hexGridManager.StartGame();
    }

    public void SetGameBoard()
    {
        if (IsServer)
        {
            if (!hexGridManager.IsInitialized)
            {
                hexGridManager.Initialize();
            }
            GeneratedBoardData boardData = hexGridManager.boardData;
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
        hexGridManager.boardData = boardData;
        hexGridManager.Initialize();
    }
    public void OnPlayerConnected()
    {
        Debug.Log("Player connected");
    }
}
