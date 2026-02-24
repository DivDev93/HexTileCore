using DG.Tweening.Core.Easing;
using Reflex.Attributes;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IGamePlayer
{
    public bool ExecutedPlacementCommand { get; set; }
    public int PlayerIndex { get; set; }
    //public string playerName { get; set; }
    public List<IPlayerCard> Cards { get; set; }
    public CommandInvoker Commands { get; set; }
    public void AddCard(PlayableCard card);
    public void RemoveCard(PlayableCard card);
    public bool IsLocalPlayer { get; }
    /// <summary>
    /// When true, the game mode will automatically resolve actions for this player (e.g. AI-controlled players).
    /// </summary>
    public bool RequiresAutoResolve { get; }
    public PlaceOnBoardCommand LastPlacementCommand { get => Commands.GetLastCommand() as PlaceOnBoardCommand; }
    public void ExecuteAction(ICardAction action);
    /// <summary>
    /// Executes the action and optionally triggers action resolution for the current turn.
    /// Pass <c>false</c> to defer action resolution, enabling multiple actions to be queued before resolution.
    /// </summary>
    public void ExecuteAction(ICardAction action, bool resolveAction);
}


public class HexPlayer : MonoBehaviour, IGamePlayer
{
    [Inject]
    public IGameManager gameManager;

    [Inject]
    public ICardSpawner Spawner;
    List<IPlayerCard> playableCards = new List<IPlayerCard>();

    public CommandInvoker Commands { get; set; }
    public bool executedCommandThisTurn = false;
    public bool ExecutedPlacementCommand { get => executedCommandThisTurn; set => executedCommandThisTurn = value; }

    int playerIndex;
    public int PlayerIndex { get => playerIndex; set => playerIndex = value; }

    protected virtual void Awake()
    {
        Commands = new CommandInvoker();
    }

    protected IPlayerCard currentCard;

    //return the playableCards
    public List<IPlayerCard> Cards
    {
        get
        {
            return playableCards;
        }
        set
        {
            playableCards = value;
        }
    }

    public virtual bool IsLocalPlayer
    {
        get
        {
            if(NetworkManager.Singleton == null || NetworkManager.Singleton.ConnectedClients.Count == 0)
            {
                return this is HexPlayer;
            }
            
            return NetworkManager.Singleton.LocalClient.PlayerObject == gameObject;
        }
    }

    public virtual bool RequiresAutoResolve => false;

    public void EndTurn()
    {
        gameManager.EndTurn();
        ExecutedPlacementCommand = false;
        Commands.Clear();
    }

    public void UndoLastMove()
    {
        Commands.UndoCommand();
    }

    public virtual void AddCard(PlayableCard card)
    {
        playableCards.Add(card);
        card.placeable.player = this;
        currentCard = card;
    }

    public virtual void RemoveCard(PlayableCard card)
    {
        playableCards.Remove(card);
    }

    public virtual void ExecuteAction(ICardAction action)
    {
        ExecuteAction(action, true);
    }

    public virtual void ExecuteAction(ICardAction action, bool resolveAction)
    {
        if (action == null || !action.CanExecute())
        {
            return;
        }

        Commands.ExecuteCommand(action);

        if (resolveAction)
        {
            gameManager.ResolveActionForCurrentPlayer();
        }
    }
}
