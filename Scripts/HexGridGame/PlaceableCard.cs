using HexTiles;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableCard : BoardPlaceable
{
    public EElementType cardElementType;

    [Inject] private IStaticEvents staticEvents;
    [Inject] private List<ElementalStrengths> strengths;

    private Outline outline;
    private bool placedForFirstTime;

    public List<Transform> OutlineTransforms;
    public Action<bool> OnElementalTileChange = default;

    protected virtual void Awake()
    {
        outline = GetComponentInChildren<Outline>();
        RegisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        OnHighlightChange += HighlightChanged;
        OnPlacedTileChange += PlacedTileChanged;
        staticEvents.OnTurnEnd += OnTurnEnd;
        Debug.Log($"Registered end turn listener for {name}");
    }

    private void OnDestroy()
    {
        UnregisterEventHandlers();
    }

    private void UnregisterEventHandlers()
    {
        OnHighlightChange -= HighlightChanged;
        OnPlacedTileChange -= PlacedTileChanged;
        if (!placedForFirstTime)
        {
            staticEvents.OnTurnEnd -= OnTurnEnd;
        }
    }

    public void OnTurnEnd()
    {
        if (!placedForFirstTime && PlacedTarget != null)
        {
            placedForFirstTime = true;
            staticEvents.OnTurnEnd -= OnTurnEnd;
            Debug.Log($"Unregistered end turn listener for {name}");
        }
        else
        {
            Debug.Log($"Turn ended but card not placed: {name}");
        }
    }

    protected override void ExecuteSelection()
    {
        if (player.ExecutedPlacementCommand && player.LastPlacementCommand != null)
        {
            if (player.LastPlacementCommand.card != this)
            {
                player.Commands.UndoCommand();
            }
            player.ExecutedPlacementCommand = false;
        }
        base.ExecuteSelection();
    }

    private void PlacedTileChanged(IBoardSelectablePosition placedTile)
    {
        if (player is AIPlayer)
        {
            Debug.Log($"AI Player placed card on {placedTile.GridPosition}");
        }
    }

    private void HighlightChanged(IBoardSelectablePosition highlightedTile)
    {
        bool isSameElement = highlightedTile != null && cardElementType == highlightedTile.ElementType;
        OnElementalTileChange?.Invoke(isSameElement);
    }

    public override void HandleHighlightLine()
    {
        if (HighlightedTarget == null)
        {
            ClearHighlight();
            return;
        }

        UpdateHighlight();
    }

    private void ClearHighlight()
    {
        ReleaseCurrentLine();
        outline.enabled = false;
    }

    private void UpdateHighlight()
    {
        Color highlightColor = DetermineHighlightColor();
        ApplyHighlightColor(highlightColor);
        DrawHighlightLine(highlightColor);
    }

    private Color DetermineHighlightColor()
    {
        if (!(HighlightedTarget is IBoardSelectablePosition highlightedHex))
        {
            return Color.cyan;
        }

        var elementStrengths = strengths.Find(x => x.Element == cardElementType);
        if (cardElementType == highlightedHex.ElementType)
        {
            return Color.green;
        }
        else if (elementStrengths.IsWeakAgainst(highlightedHex.ElementType))
        {
            return Color.red;
        }

        return Color.cyan;
    }

    private void ApplyHighlightColor(Color color)
    {
        outline.enabled = true;
        outline.OutlineColor = color;
    }

    private void DrawHighlightLine(Color color)
    {
        Vector3[] positions = LinePositionsWithTarget(HighlightedTarget.GetTransform().position);
        currentRaycastLine = VolumetricLinePool.DrawLine(positions, color, currentRaycastLine);
    }

    protected Vector3[] LinePositionsWithTarget(Vector3 targetPos)
    {
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
        {
            base.ClickPlacedTile();
        }
        else
        {
            gameBoard.SelectStartHexTilesForPlayer(player.PlayerIndex);
        }
    }

    public override void OnBeginDrag()
    {
        if (AllowInteraction())
        {
            base.OnBeginDrag();
        }
    }
}
