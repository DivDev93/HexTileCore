using HexTiles;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableCard : BoardPlaceable
{

    public EElementType cardElementType;

    [Inject]
    IGameManager gameManager;

    [Inject]
    List<ElementalStrengths> strengths;

    Outline outline;

    public List<Transform> OutlineTransforms;

    public Action<bool> OnElementalTileChange = default;

    public void Awake()
    {
        outline = GetComponentInChildren<Outline>();
        OnHighlightChange += HighlightChange;
    }

    private void OnDestroy()
    {
        OnHighlightChange -= HighlightChange;
    }

    bool placedForFirstTime = false;

    public override void OnTargetPlace()
    {
        base.OnTargetPlace();
        if (!placedForFirstTime)
        {
            placedForFirstTime = true;
            //gameManager.EndTurn();
        }
    }

    void HighlightChange(ISelectableTarget selectableTarget)
    {
        OnElementalTileChange.Invoke(selectableTarget == null? false : cardElementType == ((IBoardSelectablePosition)selectableTarget).ElementType);
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
}
