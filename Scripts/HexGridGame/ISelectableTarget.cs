using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectableTarget
{
    public void OnHoverEnter();
    public void OnHoverExit();
    public void OnTileClick();
    public Action OnTilePulse { get; set; }
    public Transform GetTransform();
    public bool IsSelected { get; set; }
    public ISelectableTarget selectableTarget { get; }
    public void AddNeighbor(IBoardSelectablePosition neighbor);
    public int SelectNeighbors(int step, out List<IBoardSelectablePosition> selectedTiles);
    public void ClearHighlight();
    public void PulseSelect(PulseData pulseData, float delay = 0f);
}

