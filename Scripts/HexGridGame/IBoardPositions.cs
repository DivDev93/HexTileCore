using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityLabs.Slices.Games.Chess;
using UnityEngine.Jobs;
using UnityEditor.Experimental.GraphView;

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
    public List<IBoardPosition> positionList { get; }
    public TransformAccessArray transformAccessArray { get; }
    public bool isBoardCreated { get; set; }
}

public interface IBoardPosition
{
    public bool IsHighlighted { get; set; }
    public Transform transform { get; }
    public Vector3 originalPos { get; set; }
    public bool IsSelected { get; set; }
    public ISelectableTarget selectableTarget { get; }
}