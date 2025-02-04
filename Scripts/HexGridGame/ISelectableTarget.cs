using System;
using UnityEngine;

public interface ISelectableTarget
{
    public void OnHoverEnter();
    public void OnHoverExit();
    public void OnTileClick();
    public Action OnTilePulse { get; set; }
    public Transform GetTransform();
}

