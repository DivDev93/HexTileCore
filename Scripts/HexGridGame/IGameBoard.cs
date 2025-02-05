using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using System;

public interface IGameBoard
{
    public void Initialize();
    public void SetPiecesVisibility(bool state);
    public BindableVariable<float> boardRotation { get; }
    public void ClaimBoardStateOwnership();
    public void StartGame(VersusGameMode mode, bool isLocal);
    public IBoardPositions boardPositions { get; }
    public TileGameDataScriptableObject tileGameData { get; }
    public List<IBoardPosition> selectedTiles { get; }
    public bool TryGetTile(Collider collider, out IBoardPosition tile);
    public void SelectStartHexTilesForPlayer(int playerIndex);
    public GeneratedBoardData boardData { get; set; }

}
