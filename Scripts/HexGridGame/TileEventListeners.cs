using System;
using UnityEngine;

public interface IStaticEvents
{
    public Action OnGameStarted { get; set; }
    public Action<IBoardSelectablePosition> OnTileHoverEnter { get; set; }
    public Action<IBoardSelectablePosition> OnTileHoverExit { get; set; }
    public Action<IBoardSelectablePosition> OnTileClicked { get; set; }

}

public class TileEventListeners : IStaticEvents
{
    public Action m_OnGameStarted;
    public Action OnGameStarted { get => m_OnGameStarted; set => m_OnGameStarted = value; }
    public Action<IBoardSelectablePosition> OnTileHoverEnter { get => (Action<IBoardSelectablePosition>)HexTile.OnTileHoverEnter; set => HexTile.OnTileHoverEnter = value; }
    public Action<IBoardSelectablePosition> OnTileHoverExit { get => (Action<IBoardSelectablePosition>)HexTile.OnTileHoverExit; set => HexTile.OnTileHoverExit = value; }
    public Action<IBoardSelectablePosition> OnTileClicked { get => (Action<IBoardSelectablePosition>)HexTile.OnTileClicked; set => HexTile.OnTileClicked = value; }

}
