using HexTiles;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableCard : BoardPlaceable
{
    public EElementType cardElementType;

    [Inject]
    IStaticEvents staticEvents;

    [Inject]
    List<ElementalStrengths> strengths;

    Outline outline;

    public List<Transform> OutlineTransforms;

    public Action<bool> OnElementalTileChange = default;

    public void Awake()
    {
        outline = GetComponentInChildren<Outline>();
        OnHighlightChange += HighlightChanged;
        OnPlacedTileChange += PlacedTileChanged;
        staticEvents.OnTurnEnd += OnTurnEnd;
        Debug.Log("should register end turn listener" + name);
    }

    private void OnDestroy()
    {
        OnHighlightChange -= HighlightChanged;
        OnPlacedTileChange -= PlacedTileChanged;
    }

    bool placedForFirstTime = false;

    public void OnTurnEnd()
    {
        if (!placedForFirstTime && PlacedTarget != null)
        {
            placedForFirstTime = true;
            staticEvents.OnTurnEnd -= OnTurnEnd;
            //gameManager.EndTurn();
            Debug.Log("should unregister end turn listener " + name);
        }
        else
            Debug.Log("Turn ended but card not placed " + name);
    }

    protected override void ExecuteSelection()
    {
        if (player.ExecutedPlacementCommand && player.LastPlacementCommand != null)
        {
            if(player.LastPlacementCommand.card != this)
                player.Commands.UndoCommand();

            player.ExecutedPlacementCommand = false;
        }
        base.ExecuteSelection();
    }

    void PlacedTileChanged(IBoardSelectablePosition placedTile)
    {
        if(player is AIPlayer)
            Debug.Log("AI Player PLACED Card on " + placedTile.GridPosition);

    }

    void HighlightChanged(IBoardSelectablePosition highlightedTile)
    {
        OnElementalTileChange.Invoke(highlightedTile == null? false : cardElementType == highlightedTile.ElementType);
    }

    public override void HandleHighlightLine()
    {
        if (HighlightedTarget == null)
        {
            ReleaseCurrentLine();
            outline.enabled = false;
        }
        else 
        {
            Color col = Color.cyan;
            if (HighlightedTarget is IBoardSelectablePosition highlightedHex)
            {
                var elementStrengths = strengths.Find(x => x.Element == cardElementType);
                if (cardElementType == highlightedHex.ElementType) //(elementStrengths.IsStrongAgainst(highlightedHex.tileType))
                {
                    col = Color.green;
                }
                else if (elementStrengths.IsWeakAgainst(highlightedHex.ElementType))
                {
                    col = Color.red;
                }
            }

            outline.enabled = true;
            outline.OutlineColor = col;

            currentRaycastLine = VolumetricLinePool.DrawLine(LinePositionsWithTarget(HighlightedTarget.GetTransform().position), col, currentRaycastLine);
            //Debug.Log("DRAWING LINE");
            //currentRaycastLine = VolumetricLinePool.DrawLine(transform.position, HighlightedTarget.GetTransform().position, col, currentRaycastLine);
        }
    }

    Vector3[] LinePositionsWithTarget(Vector3 targetPos)
    {
        //add targetPos position to end of Line Positions
        Vector3[] positions = new Vector3[OutlineTransforms.Count + 1];
        for (int i = 0; i < OutlineTransforms.Count; i++)
        {
            positions[i] = OutlineTransforms[i].position;
        }
        positions[OutlineTransforms.Count] = targetPos;
        return positions;
    }

    public override void ClickPlacedTile()
    {
        if (placedForFirstTime)
            base.ClickPlacedTile();
        else
            gameBoard.SelectStartHexTilesForPlayer(player.PlayerIndex);
        //Debug.Log("Card Selected");
        //gameManager.EndTurn();
    }

    public override void OnBeginDrag()
    {
        if(AllowInteraction())
            base.OnBeginDrag();
    }
}
