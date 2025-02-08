﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
using UnityUtils;
using Reflex.Attributes;
using JSAM;

public class HexTile : MonoBehaviour, IPointerClickHandler, IBoardSelectablePosition, ISelectableTarget
{
    [Inject]
    IGameBoard gameBoard;

    Vector2Int m_gridPosition;
    public Vector2Int GridPosition { get => m_gridPosition; set => m_gridPosition = value; } // Axial coordinate
    public List<IBoardSelectablePosition> Neighbors { get; private set; } = new List<IBoardSelectablePosition>();
    
    bool isSelected = false;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            if (isSelected)
            {
                ApplySelectionHighlight();
            }
            else
            {
                ClearHighlight();
            }
        }
    }


    bool isHighlighted = false;
    bool IBoardSelectablePosition.IsHighlighted 
    { 
        get => isHighlighted;
        set 
        { 

            isHighlighted = value; 
        } 
    }

    Transform IBoardSelectablePosition.transform => gameObject.transform;

    Vector3 m_originalPos;

    public Vector3 originalPos { get => m_originalPos; set => m_originalPos = value; }
    public Action OnTilePulse { get => onTilePulse; set => onTilePulse = value; }

    public ISelectableTarget selectableTarget => this;

    public Color selectColor = Color.yellow;
    public Color hoverColor = Color.cyan;
    MeshRenderer tileRenderer;
    //AudioSource audioSource;
    bool ignoreHighlight = false;

    public static Action<HexTile> OnTileClicked;
    Action onTilePulse;
    public static Action<HexTile> OnTileHoverEnter;
    public static Action<HexTile> OnTileHoverExit;

    void Awake()
    {
        tileRenderer = GetComponent<MeshRenderer>();
        //audioSource = GetComponent<AudioSource>();
    }
    // Initialize tile
    public void Initialize(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        gameObject.name = $"Hex ({gridPosition.x}, {gridPosition.y})";
        //gameObject.AddComponent<MeshCollider>().convex = true;
    }

    // Add a neighbor to this tile
    public void AddNeighbor(HexTile neighbor)
    {
        if (!Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
        }
    }

    public void OnTileClick()
    {
        ignoreHighlight = true;
        OnTileClicked?.Invoke(this);
        ignoreHighlight = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClick();
    }

    //Recursive function to highlight neighbors
    public int SelectNeighbors(int step, out List<IBoardSelectablePosition> selectedTiles)
    {
        selectedTiles = new List<IBoardSelectablePosition>();
        if (step == 0)
        {
            return 0;
        }

        //Debug.Log($"Highlighting neighbors with out at step {step}");
        foreach (var neighbor in Neighbors)
        {
            // Add a delay based on the step
            neighbor.SelectNeighbors(step - 1, out List<IBoardSelectablePosition> subSelectedTiles);
            HexUtility.AddUniqueRange(selectedTiles, subSelectedTiles);
        }

        HexUtility.AddUniqueRange(selectedTiles, Neighbors);
        selectedTiles.RemoveAll(tile => tile == this);
        return step - 1;
    }

    //public int HighlightNeighbors(int step)
    //{
    //    if (step == 0)
    //    {
    //        return 0;
    //    }

    //    Debug.Log($"Highlighting neighbors at step {step}");
    //    foreach (var neighbor in Neighbors)
    //    {
    //        // Add a delay based on the step
    //        float neighborDelay = pulseData.delay * (step - 1);
    //        neighbor.Pulse(neighborDelay);
    //        neighbor.Highlight();
    //        neighbor.HighlightNeighbors(step - 1);
    //    }

    //    HexGridManager.Instance.selectedTiles.AddRange(Neighbors);
    //    return step - 1;
    //}

    public void ApplySelectionHighlight()
    {
        //if (!ignoreHighlight)
        {
            //tileRenderer.material.color = selectColor;
            isHighlighted = false;
            //Debug.Log($"Highlighted Hex: {GridPosition}");
        }
    }

    public bool isPulsing = false;
    public void OnHoverEnter()
    {
        isHighlighted = true;
        if (!isPulsing)
        {
            isPulsing = true;
            Sequence sequence = Sequence.Create();
            sequence.AppendCallback(() =>
            {
                transform.PulseY(transform.localPosition.With(y: 0f), gameBoard.tileGameData.pulseData.height * gameBoard.tileGameData.parentScale, 1, gameBoard.tileGameData.pulseData.duration, true).SetEase(Ease.Linear);
                if (!AudioManager.IsSoundPlaying(HexTileAudioLibSounds.Clunk, transform))
                    AudioManager.PlaySound(HexTileAudioLibSounds.Clunk, transform);
            }
            );
            sequence.OnComplete(() => isPulsing = false);
            sequence.Play();
            //Debug.Log("Pulsing" + GridPosition);
        }
        OnTileHoverEnter?.Invoke(this);
    }

    public void PulseSelect(PulseData pulseData, float delay = 0f)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(delay * pulseData.delay);
        sequence.Append(PrimeTweenExtensions.PulseY(transform, transform.localPosition.With(y: 0f), pulseData.height * gameBoard.tileGameData.parentScale, 1, pulseData.duration, true).SetEase(Ease.Linear));
        sequence.AppendCallback(() =>
        {
            OnTilePulse?.Invoke();
            if(!AudioManager.IsSoundPlaying(HexTileAudioLibSounds.Clunk, transform))
                AudioManager.PlaySound(HexTileAudioLibSounds.Clunk, transform);
        });
        sequence.OnComplete(() =>
        {
            if (delay != 0f && gameBoard.selectedTiles.Contains(this))
            {
                IsSelected = true;
            }
        });
        sequence.Play();
    }

    public void ClearHighlight()
    {
        //tileRenderer.material.color = Color.white;
        isHighlighted = false;
    }

    public void OnHoverExit()
    {
        if (!gameBoard.selectedTiles.Contains(this))
        {
            ClearHighlight();
        }
        else
        {
            IsSelected = true;
        }
        OnTileHoverExit?.Invoke(this);
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

