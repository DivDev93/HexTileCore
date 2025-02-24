using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : HexPlayer
{
    public int aiTurnDelay = 1500;
    [Inject]
    public IGameBoard gameBoard;

    [Inject]
    public ICardSpawner cardSpawner;

    [Inject]
    IStaticEvents staticEvents;

    public override bool IsLocalPlayer => false;

    public AIPlayer()
    {
        PlayerIndex = 1;
    }

    void Start()
    {
        base.Awake();
        staticEvents.OnTurnStart += StartTurn;
    }

    private void OnDestroy()
    {
        staticEvents.OnTurnStart -= StartTurn;
    }
    public void StartTurn()
    {
        if(gameManager.CurrentPlayerTurn != PlayerIndex)
        {
            return;
        }
        Debug.Log("AI Player Turn");
        PlayCard();

    }

    async void PlayCard()
    {
        while(cardSpawner.CurrentSpawnedCard == null)
        {
            await UniTask.Delay(250);
        }

        RandomizeCurrentCard();
        IBoardSelectablePosition selectedTile;
        if (currentCard.placeable.PlacedTarget == null)
        {
            selectedTile = GetMatchingElementFromSelectedTiles();
        }
        else
        {
            selectedTile = GetMatchingFromNeighbors();
        }

        currentCard.placeable.HighlightedTarget = selectedTile;
        currentCard.placeable.transform.SetParent(null);
        currentCard.placeable.OnSelectExit();
        currentCard.placeable.OnPlacedTileChange += PlacedTileChanged;

        Debug.Log("AI Player Played Card on " + selectedTile.GridPosition);
    }

    void RandomizeCurrentCard()
    {
        int guid = System.Guid.NewGuid().GetHashCode() + Time.frameCount;
        //Either randomly choose to play a card from the spawner or a random one from the hand
        if (Cards.Count == 0 || guid/2 == 0)
        {
            Debug.Log("AI PLAYER CHOSE SPAWNED CARD");
            currentCard = cardSpawner.CurrentSpawnedCard;
        }
        else
        {
            Debug.Log("AI PLAYER CHOSE RANDOM CARD FROM HAND");
            currentCard = ChooseRandomCardFromHand();
        }
    }

    IPlayerCard ChooseRandomCardFromHand()
    {
        int guid = System.Guid.NewGuid().GetHashCode();
        Random.InitState(guid + Time.frameCount);
        return Cards[Random.Range(0, Cards.Count)];
    }

    public void PlacedTileChanged(IBoardSelectablePosition placedTile)
    {
        currentCard.placeable.OnPlacedTileChange -= PlacedTileChanged;
        EndTurn();
    }

    IBoardSelectablePosition GetMatchingFromNeighbors()
    {
        if(currentCard.placeable.PlacedTarget == null)
        {
            return null;
        }

        var neighbors = currentCard.GetNeighborsFromPlacedTile();
        var matchingTile = GetMatchingElementFrom(neighbors);

        if (matchingTile != null)
        {
            Debug.Log("FOUND MATCHING ELEMENT in NEIGHBORS with tile type " + matchingTile.ElementType + " CARD TYPE IS " + currentCard.CardType);
            return matchingTile;
        }

        return GetRandomFromTiles(neighbors);
    }

    IBoardSelectablePosition GetMatchingElementFromSelectedTiles()
    {
        var matchingTile = GetMatchingElementFrom(gameBoard.SelectedTiles);

        if (matchingTile != null)
        {
            Debug.Log("FOUND MATCHING ELEMENT in STARTING SELECTION with tile type " + matchingTile.ElementType + " CARD TYPE IS " + currentCard.CardType);
            return matchingTile;
        }
        return GetRandomFromTiles(gameBoard.SelectedTiles);
    }

    IBoardSelectablePosition GetMatchingElementFrom(List<IBoardSelectablePosition> selectedTiles)
    {
        foreach (var tile in selectedTiles)
        {
            if (tile.ElementType == currentCard.CardType)
            {
                Debug.Log("AI Player Found Matching Element of type " + tile.ElementType);
                return tile;
            }
        }
        return null;
    }

    IBoardSelectablePosition GetRandomFromTiles(List<IBoardSelectablePosition> selectedTiles)
    {
        int guid = System.Guid.NewGuid().GetHashCode();
        Random.InitState(guid + Time.frameCount);
        return selectedTiles[Random.Range(0, gameBoard.SelectedTiles.Count)];
    }
}
