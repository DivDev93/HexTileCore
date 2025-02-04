using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
using UnityUtils;
using Reflex.Attributes;
using JSAM;

public class HexTile : MonoBehaviour, IPointerClickHandler, IBoardPosition
{
    [Inject]
    HexGridManager hexGridManager;

    public Vector2Int GridPosition; // Axial coordinate
    public List<HexTile> Neighbors { get; private set; } = new List<HexTile>();
    
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
    bool IBoardPosition.IsHighlighted 
    { 
        get => isHighlighted;
        set 
        { 

            isHighlighted = value; 
        } 
    }

    Transform IBoardPosition.transform => gameObject.transform;

    Vector3 m_originalPos;

    public Vector3 originalPos { get => m_originalPos; set => m_originalPos = value; }

    public Color selectColor = Color.yellow;
    public Color hoverColor = Color.cyan;
    MeshRenderer tileRenderer;
    //AudioSource audioSource;
    bool ignoreHighlight = false;

    public static Action<HexTile> OnTileClicked;
    public Action OnTilePulse;
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
    public int SelectNeighbors(int step, out List<HexTile> selectedTiles)
    {
        selectedTiles = new List<HexTile>();
        if (step == 0)
        {
            return 0;
        }

        //Debug.Log($"Highlighting neighbors with out at step {step}");
        foreach (var neighbor in Neighbors)
        {
            // Add a delay based on the step
            neighbor.SelectNeighbors(step - 1, out List<HexTile> subSelectedTiles);
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

    public void OnHoverEnter()
    {
        isHighlighted = true;
        //tileRenderer.material.color = hoverColor;
        OnTileHoverEnter?.Invoke(this);
    }

    public void PulseSelect(PulseData pulseData, float delay = 0f)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(delay * pulseData.delay);
        sequence.Append(PrimeTweenExtensions.PulseY(transform, transform.localPosition.With(y: 0f), pulseData.height * hexGridManager.parentScale, 1, pulseData.duration, true).SetEase(Ease.Linear));
        sequence.AppendCallback(() =>
        {
            OnTilePulse?.Invoke();
            if(!AudioManager.IsSoundPlaying(HexTileAudioLibSounds.Clunk, transform))
                AudioManager.PlaySound(HexTileAudioLibSounds.Clunk, transform);
        });
        sequence.OnComplete(() =>
        {
            if (delay != 0f && hexGridManager.selectedTiles.Contains(this))
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
        if (!hexGridManager.selectedTiles.Contains(this))
        {
            ClearHighlight();
        }
        else
        {
            IsSelected = true;
        }
        OnTileHoverExit?.Invoke(this);
    }
}

