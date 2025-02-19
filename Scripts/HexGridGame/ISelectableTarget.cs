using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectableTarget
{
    public void OnHoverEnter();
    public void OnHoverExit();
    public void OnSelectionClick();
    public Transform GetTransform();
    public bool IsSelected { get; set; }
    public void ClearHighlight();
    public Action OnSelect { get; set; }
}

