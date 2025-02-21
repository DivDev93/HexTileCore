using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

[Serializable]
public class DebugPosScale
{
    public Vector3 position;
    public float scale;
}

public class InstancedSelectionRenderer : MonoBehaviour
{
    [Inject]
    IGameBoard gameBoard;

    [Inject]
    IStaticEvents staticEvents;

    public Color selectionColor;
    public float initAlpha = 0.25f;
    public float blinkSpeed = 1.0f;
    public bool cacheRenderParams = false;
    public Material selectMaterial, hoverMaterial;
    public Mesh mesh;
    public Vector3 selectedScale = Vector3.up * 0.7f;
    public Vector3 hoveredScale = Vector3.up * 1.4f;
    public Vector3 offset = Vector3.up;
    int numInstances = 0;
    private List<Matrix4x4> instData = new List<Matrix4x4>();
    private List<IBoardSelectablePosition> hoveredTiles = new List<IBoardSelectablePosition>();
    private Matrix4x4[] renderMatrices;
    private Matrix4x4[] instDataArray;
    RenderParams rpSelected, rpHover;

    private void Start()
    {
        rpSelected = new RenderParams(selectMaterial);
        rpHover = new RenderParams(hoverMaterial);
    }

    void Update()
    {
        if (gameBoard.boardPositions.isBoardCreated)
        {
            UpdateSelectedTiles();
            RenderHexTilesInstanced(hoveredTiles, rpHover);
        }
    }

    void UpdateSelectedTiles()
    {
        numInstances = gameBoard.SelectedTiles.Count;

        if (!cacheRenderParams)
            rpSelected = new RenderParams(selectMaterial);

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_BaseColor", selectionColor * Mathf.PingPong(Time.time * blinkSpeed, initAlpha));
        rpSelected.matProps = propertyBlock;
        AdjustInstanceData();

        for (int i = 0; i < numInstances; ++i)
        {
            AddSelectedIndex(i, false);
        }

        if (numInstances > 0)
        {
            if (instDataArray == null || instDataArray.Length < numInstances)
            {
                instDataArray = new Matrix4x4[numInstances];
            }
            instData.CopyTo(instDataArray);
            Graphics.RenderMeshInstanced(rpSelected, mesh, 0, instDataArray, numInstances);
        }
    }

    void AddSelectedIndex(int i, bool add = true)
    {
        if (!gameBoard.SelectedTiles[i].IsSelected || hoveredTiles.Contains(gameBoard.SelectedTiles[i]))
        {
            if (add)
                instData.Add(new Matrix4x4());
            return;
        }

        Vector3 translation = gameBoard.SelectedTiles[i].transform.position.With(y: transform.position.y) + offset;
        Vector3 matrixScale = gameBoard.tileGameData.parentScale * selectedScale;
        Matrix4x4 inst = Matrix4x4.TRS(translation, Quaternion.identity, matrixScale);

        if (add)
        {
            instData.Add(inst);
        }
        else
        {
            instData[i] = inst;
        }
    }

    void RenderHexTilesInstanced(List<IBoardSelectablePosition> tiles, RenderParams rp)
    {
        if (renderMatrices == null || renderMatrices.Length < tiles.Count)
        {
            renderMatrices = new Matrix4x4[tiles.Count];
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            Vector3 translation = tiles[i].transform.position.With(y: transform.position.y) + offset;
            Vector3 matrixScale = gameBoard.tileGameData.parentScale * hoveredScale;
            Matrix4x4 inst = Matrix4x4.TRS(translation, Quaternion.identity, matrixScale);
            renderMatrices[i] = inst;
        }

        if (tiles.Count > 0)
            Graphics.RenderMeshInstanced(rp, mesh, 0, renderMatrices, tiles.Count);
    }

    void AdjustInstanceData()
    {
        if (instData.Count == numInstances)
            return;

        if (instData.Count < numInstances)
        {
            for (int i = instData.Count; i < numInstances; i++)
            {
                AddSelectedIndex(i);
            }
        }
        else if (instData.Count > numInstances)
        {
            instData.RemoveRange(numInstances, instData.Count - numInstances);
        }
    }

    void OnHoverTileEnter(IBoardSelectablePosition tile)
    {
        if (!hoveredTiles.Contains(tile))
        {
            hoveredTiles.Add(tile);
        }
    }

    void OnHoverTileExit(IBoardSelectablePosition tile)
    {
        if (hoveredTiles.Contains(tile))
        {
            hoveredTiles.Remove(tile);
        }
    }

    public virtual void OnEnable()
    {
        staticEvents.OnTileHoverEnter += OnHoverTileEnter;
        staticEvents.OnTileHoverExit += OnHoverTileExit;
    }

    public virtual void OnDisable()
    {
        staticEvents.OnTileHoverEnter -= OnHoverTileEnter;
        staticEvents.OnTileHoverExit -= OnHoverTileExit;
    }
}
