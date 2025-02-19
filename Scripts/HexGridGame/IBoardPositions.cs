using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityLabs.Slices.Games.Chess;
using UnityEngine.Jobs;
using System;

public interface IGameUI
{
    public void SetStartButtonsActive(bool state);
}
public enum VersusGameMode : byte
{
    NotStarted = 0,
    HumanVsHuman = 1,
    HumanVsAI = 2,
    AIvsHuman = 3,
    AIvsAI = 4
}

public interface IBoardPositions
{ 
    public GameObject boardGameObject { get; }
    public List<IBoardSelectablePosition> positionList { get; }
    public TransformAccessArray transformAccessArray { get; }
    public bool isBoardCreated { get; set; }
}

public interface IBoardPosition
{
    public Vector2Int GridPosition { get; set; }
    public bool IsHighlighted { get; set; }
    public Transform transform { get; }
    public Vector3 originalPos { get; set; }
}

public interface IBoardSelectablePosition : ISelectableTarget, IBoardPosition
{
    public void AddNeighbor(IBoardSelectablePosition neighbor);
    public int SelectNeighbors(int step, out List<IBoardSelectablePosition> selectedTiles);
    public void PulseSelect(PulseData pulseData, float delay = 0f);
}