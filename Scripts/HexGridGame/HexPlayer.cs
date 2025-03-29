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
    public PlaceOnBoardCommand LastPlacementCommand { get => Commands.GetLastCommand() as PlaceOnBoardCommand; }
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

    public void EndTurn()
    {
        gameManager.EndTurn();
        ExecutedPlacementCommand = false;
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
}
