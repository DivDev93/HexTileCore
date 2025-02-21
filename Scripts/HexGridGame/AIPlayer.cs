using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

public class AIPlayer : HexPlayer
{
    [Inject]
    public IGameBoard gameBoard;

    [Inject]
    public ICardSpawner cardSpawner;

    [Inject]
    IStaticEvents staticEvents;

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
        EndTurn();
    }

    async void PlayCard()
    {
        while(cardSpawner.CurrentSpawnedCard == null)
        {
            await UniTask.Delay(250);
        }

        IBoardSelectablePosition selectedTile = GetMatchingElementFromSelectedTiles();
        cardSpawner.CurrentSpawnedCard.placeable.HighlightedTarget = selectedTile;
        cardSpawner.CurrentSpawnedCard.placeable.transform.SetParent(null);
        cardSpawner.CurrentSpawnedCard.placeable.OnSelectExit();

        Debug.Log("AI Player Played Card on " + selectedTile.GridPosition);
    }

    IBoardSelectablePosition GetMatchingElementFromSelectedTiles()
    {
        foreach (var tile in gameBoard.SelectedTiles)
        {
            if (tile.ElementType == cardSpawner.CurrentSpawnedCard.placeable.cardElementType)
            {
                Debug.Log("AI Player Found Matching Element of type " + tile.ElementType);
                return tile;
            }
        }
        return GetRandomFromSelectedTiles();
    }

    IBoardSelectablePosition GetRandomFromSelectedTiles()
    {
        int guid = System.Guid.NewGuid().GetHashCode();
        Random.InitState(guid + Time.frameCount);
        return gameBoard.SelectedTiles[Random.Range(0, gameBoard.SelectedTiles.Count)];
    }
}
