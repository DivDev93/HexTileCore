using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class HexGameManager : MonoBehaviour, IGameManager
{
    [Inject]
    IGameBoard gameBoard;
    public int currentPlayerTurn;
    public List<HexPlayer> players = new List<HexPlayer>();

    public void StartGame()
    {
        gameBoard.SelectStartHexTilesForPlayer(0);
        gameBoard.StartGame(VersusGameMode.HumanVsAI, true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameBoard.Initialize();
        gameBoard.SelectStartHexTilesForPlayer(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
