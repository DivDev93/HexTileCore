using System;
using UnityEngine;

public interface IStaticEvents
{
    public Action<IBoardPosition> OnTileHoverEnter { get; set; }
    public Action<IBoardPosition> OnTileHoverExit { get; set; }
}

public class TileEventListeners : IStaticEvents
{
    public Action<IBoardPosition> OnTileHoverEnter { get => (Action<IBoardPosition>)HexTile.OnTileHoverEnter; set => HexTile.OnTileHoverEnter = value; }
    public Action<IBoardPosition> OnTileHoverExit { get => (Action<IBoardPosition>)HexTile.OnTileHoverExit; set => HexTile.OnTileHoverExit = value; }

}
