using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityLabs.Slices.Games.Chess;

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

public interface IGameBoard
{
    public void SetPiecesVisibility(bool state);
    public BindableVariable<float> boardRotation { get; }
    public void ClaimBoardStateOwnership();
    public void StartGame(VersusGameMode mode, bool isLocal);
}

public interface IBoardPositions
{ 
    public GameObject boardGameObject { get; }
    public List<IBoardPosition> positionList { get; }
}

public interface IBoardPosition
{
    public bool IsHighlighted { get; set; }
    public Transform transform { get; }
    public Vector3 originalPos { get; set; }
}
