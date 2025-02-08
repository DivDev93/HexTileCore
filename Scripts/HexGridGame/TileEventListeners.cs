using System;
using UnityEngine;

public interface IStaticEvents
{
    public Action<IBoardSelectablePosition> OnTileHoverEnter { get; set; }
    public Action<IBoardSelectablePosition> OnTileHoverExit { get; set; }
}

public class TileEventListeners : IStaticEvents
{
    public Action<IBoardSelectablePosition> OnTileHoverEnter { get => (Action<IBoardSelectablePosition>)HexTile.OnTileHoverEnter; set => HexTile.OnTileHoverEnter = value; }
    public Action<IBoardSelectablePosition> OnTileHoverExit { get => (Action<IBoardSelectablePosition>)HexTile.OnTileHoverExit; set => HexTile.OnTileHoverExit = value; }

}
