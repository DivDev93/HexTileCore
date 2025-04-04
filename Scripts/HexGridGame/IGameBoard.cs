﻿using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using System;

public interface IGameBoard
{
    public void Initialize();
    public void SetPiecesVisibility(bool state);
    public void ClaimBoardStateOwnership();
    public void OnGameStart(VersusGameMode mode, bool isLocal);
    public IBoardPositions boardPositions { get; }
    public TileGameDataScriptableObject tileGameData { get; }
    public List<IBoardSelectablePosition> SelectedTiles { get; }
    public void SelectStartHexTilesForPlayer(int playerIndex);
    public GeneratedBoardData boardData { get; set; }
    public Dictionary<Collider, IBoardSelectablePosition> tileColliderDict { get; }
    public Dictionary<Vector2Int, BoardPlaceable> GridPlaceables { get; set; }
    public IBoardSelectablePosition GetTile(Vector2Int gridPosition);
}
