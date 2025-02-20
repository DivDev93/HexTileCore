using DG.Tweening.Core.Easing;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePlayer
{
    public int PlayerIndex { get; set; }
    //public string playerName { get; set; }
    public List<IPlayerCard> Cards { get; set; }
    public CommandInvoker Commands { get; set; }
    public void AddCard(PlayableCard card);
    public void RemoveCard(PlayableCard card);
}


public class HexPlayer : MonoBehaviour, IGamePlayer
{
    [Inject]
    public IGameManager gameManager;

    [Inject]
    public ICardSpawner Spawner;
    List<IPlayerCard> playableCards = new List<IPlayerCard>();

    public CommandInvoker Commands { get; set; }

    int playerIndex;
    public int PlayerIndex { get => playerIndex; set => playerIndex = value; }

    protected virtual void Awake()
    {
        Commands = new CommandInvoker();
    }

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

    public void EndTurn()
    {
        gameManager.EndTurn();
    }

    public void UndoLastMove()
    {
        Commands.UndoCommand();
    }

    public virtual void AddCard(PlayableCard card)
    {
        playableCards.Add(card);
        card.placeable.player = this;
    }

    public virtual void RemoveCard(PlayableCard card)
    {
        playableCards.Remove(card);
    }
}
