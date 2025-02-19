using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePlayer
{
    public int PlayerIndex { get; set; }
    //public string playerName { get; set; }
    public List<IPlayerCard> Cards { get; set; }
}


public class HexPlayer : MonoBehaviour, IGamePlayer
{
    [Inject]
    public ICardSpawner Spawner;
    List<IPlayerCard> playableCards = new List<IPlayerCard>();

    int playerIndex;
    public int PlayerIndex { get => playerIndex; set => playerIndex = value; }

    protected virtual void Awake()
    {
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

    public virtual void AddCard(PlayableCard card)
    {
        playableCards.Add(card);
    }

    public virtual void RemoveCard(PlayableCard card)
    {
        playableCards.Remove(card);
    }
}
