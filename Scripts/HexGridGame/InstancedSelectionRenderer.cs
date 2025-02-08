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

    public bool cacheRenderParams = false;
    public Material selectMaterial, hoverMaterial;
    public Mesh mesh;
    public Vector3 selectedScale = Vector3.up * 0.7f;
    public Vector3 hoveredScale = Vector3.up * 1.4f;
    public Vector3 offset = Vector3.up;
    int numInstances = 0;
    //public List<DebugPosScale> posScales = new List<DebugPosScale>();
    private List<Matrix4x4> instData = new List<Matrix4x4>();
    List<IBoardSelectablePosition> hoveredTiles = new List<IBoardSelectablePosition>();
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
            //RenderHexTilesInstanced(hexGridManager.selectedTiles, rpSelected);
            RenderHexTilesInstanced(hoveredTiles, rpHover);
        }
    }

    void UpdateSelectedTiles()
    {
        numInstances = gameBoard.selectedTiles.Count;

        if (!cacheRenderParams)
            rpSelected = new RenderParams(selectMaterial);

        AdjustInstanceData();

        for (int i = 0; i < numInstances; ++i)
        {
            AddSelectedIndex(i, false);
        }

        if (numInstances > 0)
            Graphics.RenderMeshInstanced(rpSelected, mesh, 0, instData.ToArray());
    }

    void AddSelectedIndex(int i, bool add = true)
    {
        if (!gameBoard.selectedTiles[i].IsSelected || hoveredTiles.Contains(gameBoard.selectedTiles[i]))
        {
            if (add)
                instData.Add(new Matrix4x4());
            return;
        }

        Vector3 translation = gameBoard.selectedTiles[i].transform.position.With(y: 0) + offset;
        Vector3 matrixScale = gameBoard.tileGameData.parentScale * selectedScale;
        //posScales[i].position = translation;
        //posScales[i].scale = matrixScale;
        Matrix4x4 inst = new Matrix4x4();
        inst.SetTRS(translation, Quaternion.identity, matrixScale);

        if (add)
        {
            
            instData.Add(inst);
        }
        else
        {
            instData[i] = inst;
            //Debug.Log("Should have updated data without adding to the list at " + i + " translation and scale are: " + translation + " " + matrixScale);
        }
    }

    void RenderHexTilesInstanced(List<IBoardSelectablePosition> tiles, RenderParams rp)
    {
        Matrix4x4[] renderMatrices = new Matrix4x4[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
        {
            Vector3 translation = tiles[i].transform.position.With(y: 0) + offset;
            Vector3 matrixScale = gameBoard.tileGameData.parentScale * hoveredScale;
            Matrix4x4 inst = new Matrix4x4();
            inst.SetTRS(translation, Quaternion.identity, matrixScale);
            renderMatrices[i] = inst;
        }

        if(tiles.Count > 0)
            Graphics.RenderMeshInstanced(rp, mesh, 0, renderMatrices);
    }

    void AdjustInstanceData()
    {
        if (instData.Count == numInstances)
            return;

        // Adjust the size of the list
        if (instData.Count < numInstances)
        {
            for (int i = instData.Count; i < numInstances; i++)
            {
                //posScales.Add(new DebugPosScale());
                AddSelectedIndex(i);
            }
        }
        else if (instData.Count > numInstances)
        {
            //posScales.RemoveRange(numInstances, posScales.Count - numInstances);
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
