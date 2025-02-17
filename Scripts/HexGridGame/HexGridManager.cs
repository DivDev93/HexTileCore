using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
using Reflex;
using Reflex.Attributes;
using System;
using Unity.Netcode;
using System.Collections;
//using DG.Tweening;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityLabs.Slices.Games.Chess;
using UnityEngine.Jobs;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using System.Text.RegularExpressions;
using System.Linq;



[Serializable]
public struct PulseData
{
    public float duration;
    public float delay;
    public float height;
}

[Serializable]
public struct BoardTileData : INetworkSerializable
{
    public Vector2Int gridPosition;
    public EElementType tileType;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gridPosition);
        serializer.SerializeValue(ref tileType);
    }
}

[GenerateSerializationForType(typeof(NativeList<BoardTileData>))]
public struct GeneratedBoardData : INetworkSerializable
{
    public NativeList<BoardTileData> boardTiles;

    public bool IsInitialized()
    {
        return boardTiles.IsCreated;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            // Ensure the NativeList is properly allocated before reading
            if (!boardTiles.IsCreated)
            {
                boardTiles = new NativeList<BoardTileData>(Allocator.Persistent);
            }
            serializer.SerializeValue(ref boardTiles);
        }
        else
        {
            serializer.SerializeValue(ref boardTiles);
        }
    }

    public void Dispose()
    {
        if (boardTiles.IsCreated)
        {
            boardTiles.Dispose();
        }
    }
}

public class HexGridManager : MonoBehaviour, IBoardPositions, IGameBoard//Singleton<HexGridManager>
{
    GeneratedBoardData m_boardData = new();
    public bool isBoardCreated = false;
    bool IBoardPositions.isBoardCreated { get => isBoardCreated; set => isBoardCreated = value; }

    [Inject]
    IStaticEvents staticEvents;

    [Inject]
    public StartTileIndicesScriptableObject startTileIndices;
    
    [Inject]
    public TileGameDataScriptableObject tileGameData;

    [Inject]
    HexTileFactory hexTileFactory;

    public float minScale = 0.1f;
    public float maxScale = 0.99f;
    public float noiseScale = 0.1f;

    public GameObject hexTilePrefab;// Assign the hex tile prefab
    public int movementRange = 3;

    public int gridWidth = 10;   // Width of the grid
    public int gridHeight = 10;  // Height of the grid
    float parentScale => tileGameData.parentScale; // Scale of the parent object
    [SerializeField]
    float hexSize = 0.6f;   // Size of a hex tile
    public float HexSize => hexSize * parentScale;
    public PulseData pulseData => tileGameData.pulseData;    
    bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    public bool validate = false;

    public List<IBoardSelectablePosition> selectedTiles = new List<IBoardSelectablePosition>();
    public Dictionary<Collider, IBoardSelectablePosition> m_tileColliderDict = new Dictionary<Collider, IBoardSelectablePosition>();

    private Dictionary<Vector2Int, IBoardSelectablePosition> hexTiles = new(); // Store tiles with axial coordinates
    //public HexTile[,] hexGrid;
    [Inject] IShakeable gridShake;

    List<IBoardSelectablePosition> m_positionList = new List<IBoardSelectablePosition>();
    public List<IBoardSelectablePosition> positionList
    {
        get
        {
            if (!isInitialized)
            {
                Initialize();
            }

            return m_positionList;
        }
    }

    public GameObject boardGameObject => gameObject;

    BindableVariable<float> m_BoardRotation = new BindableVariable<float>();
    public BindableVariable<float> boardRotation => m_BoardRotation;

    TransformAccessArray m_TransformsAccessArray;
    public TransformAccessArray transformAccessArray => m_TransformsAccessArray;

    public IBoardPositions boardPositions => this;

    TileGameDataScriptableObject IGameBoard.tileGameData => tileGameData;
    List<IBoardSelectablePosition> IGameBoard.selectedTiles => selectedTiles.Cast<IBoardSelectablePosition>().ToList();

    public GeneratedBoardData boardData { get => m_boardData; set => m_boardData = value; }

    public Dictionary<Collider, IBoardSelectablePosition> tileColliderDict => m_tileColliderDict;//.ToDictionary(kvp => kvp.Key, kvp => (IBoardPosition)kvp.Value);

    public UnityEvent OnStartGame = new UnityEvent();
    public UnityEvent OnBoardCreate = new UnityEvent();

    

    public void Initialize()
    {
        hexTileFactory.ShuffleTypes();
        noiseScale = UnityEngine.Random.Range(minScale, maxScale);
        isInitialized = true;
        if(boardData.IsInitialized())
        {
            GenerateGridFromBoardData();
            Debug.Log("Initializing Hex Grid from BOARDDATA");
        }
        else
        {
            GenerateHexagonalGrid();
            Debug.Log("Randomized Hex Grid");
        }
        AssignNeighbors();
        OnBoardCreate.Invoke();
    }

    void OnValidate()
    {
        if (validate)
        {
            validate = false;
            Debug.Log("hextiles count is " + hexTiles.Values.Count);
            foreach (IBoardSelectablePosition hTile in hexTiles.Values)
            {
                DelayDestroy(hTile.transform.gameObject);
            }

            DelayInit();
            Debug.Log("Hex grid generated");
        }
    }

    async void DelayInit()
    {
        await DelayInitTask();
    }

    async UniTask DelayInitTask()
    {
        await UniTask.Delay(110);
        //hexGrid = new HexTile[gridWidth, gridHeight];
        hexTiles.Clear();
        Initialize();
    }

    async void DelayDestroy(GameObject go)
    {
        await DelayedDestroyTask(go);
    }

    async UniTask DelayedDestroyTask(GameObject go)
    {
        await UniTask.Delay(100);
        Debug.Log("Destroying " + go.name);
        DestroyImmediate(go);
    }

    void OnEnable()
    {
        staticEvents.OnTileClicked += OnTileClick;
    }

    void OnDisable()
    {
        staticEvents.OnTileClicked -= OnTileClick;
    }

    void OnDestroy()
    {
        m_TransformsAccessArray.Dispose();
        boardData.boardTiles.Dispose();
    }

    //IEnumerator Start()
    //{
    //    yield return new WaitForSeconds(0.25f);
    //    Initialize();
    //}

    public void UpdateGeneratedBoardMeshes(GeneratedBoardData boardData)
    {
        foreach (var tileData in boardData.boardTiles)
        {
            IBoardSelectablePosition hexTile = GetTile(tileData.gridPosition);
            if (hexTile != null)
            {
                Mesh selectedPrefab = hexTileFactory.GetMeshForTileType(tileData.tileType);
                hexTile.transform.GetComponent<MeshFilter>().mesh = selectedPrefab;
            }
        }
    }

    public void GenerateGridFromBoardData()
    {
        foreach (var tileData in boardData.boardTiles)
        {
            // Calculate position
            float xPos = Mathf.Sqrt(3) * HexSize * (tileData.gridPosition.x + tileData.gridPosition.y / 2f);
            float yPos = 1.5f * HexSize * tileData.gridPosition.y;
            // Select prefab based on index
            Mesh selectedPrefab = hexTileFactory.GetMeshForTileType(tileData.tileType);
            // Instantiate the hex tile
            Vector3 hexPos = transform.position + new Vector3(xPos, 0, yPos);
            GameObject hexTileObject = Instantiate(hexTilePrefab, Application.isPlaying ? transform.position : hexPos, Quaternion.identity, transform);
            hexTileObject.transform.localScale = Vector3.one * parentScale;
            hexTileObject.GetComponent<MeshFilter>().mesh = selectedPrefab;
            // Add HexTile component and initialize
            HexTile hexTile = hexTileObject.GetOrAdd<HexTile>();
            hexTile.Initialize(tileData);
            // Store the tile in the dictionary
            hexTiles[tileData.gridPosition] = hexTile;
            MeshCollider collider = hexTileObject.GetComponent<MeshCollider>();
            NetworkObject networkObject = hexTileObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }
            //collider.convex = true;
            tileColliderDict[collider] = hexTile;
            hexTile.originalPos = hexPos;
            positionList.Add(hexTile);
        }
        var TransformArray = new Transform[hexTiles.Count];
        int i = 0;
        foreach (var tile in hexTiles.Values)
        {
            TransformArray[i] = tile.transform;
            i++;
            //tile.transform.parent = transform;
        }
        m_TransformsAccessArray = new TransformAccessArray(TransformArray);
        Debug.Log("Hex grid generated with " + hexTiles.Count + " tiles");
    }

    public void GenerateHexagonalGrid()
    {
        int count = 0;
        float subsectionAmount = 1f / hexTileFactory.hexMeshPrefabs.Length;

        if(!boardData.IsInitialized())
        {
            Debug.Log("Board data not initialized allocated nativelist for it");
            m_boardData.boardTiles = new NativeList<BoardTileData>(Allocator.Persistent);
        }

        //generate tiles in a hexagon shape layout with 0,0 at the center
        for (int q = -gridWidth; q <= gridWidth; q++)
        {
            int r1 = Mathf.Max(-gridHeight, -q - gridHeight);
            int r2 = Mathf.Min(gridHeight, -q + gridHeight);
            for (int r = r1; r <= r2; r++)
            {
                // Calculate position
                float xPos = Mathf.Sqrt(3) * HexSize * (q + r / 2f);
                float yPos = 1.5f * HexSize * r;

                // Sample Perlin noise
                float noiseValue = Mathf.PerlinNoise(q * noiseScale, r * noiseScale); // Scale noise to 0-250 range

                // Determine which prefab to use
                int prefabIndex = Mathf.FloorToInt(noiseValue / subsectionAmount); // Maps 0-250 into 5 sections (0-4)
                prefabIndex = Mathf.Clamp(prefabIndex, 0, hexTileFactory.hexMeshPrefabs.Length - 1); // Ensure valid index

                // Select prefab based on index
                EElementType selectedType = hexTileFactory.tileTypeArray[prefabIndex];
                Mesh selectedPrefab = hexTileFactory.hexMeshPrefabs[(int)selectedType];

                // Instantiate the hex tile
                Vector3 hexPos = transform.position + new Vector3(xPos, 0, yPos);
                GameObject hexTileObject = Instantiate(hexTilePrefab, Application.isPlaying ? transform.position : hexPos, Quaternion.identity, transform);
                hexTileObject.transform.localScale = Vector3.one * parentScale;
                hexTileObject.GetComponent<MeshFilter>().mesh = selectedPrefab;

                // Add HexTile component and initialize
                HexTile hexTile = hexTileObject.GetOrAdd<HexTile>();
                var tileData = new BoardTileData { gridPosition = new Vector2Int(q, r), tileType = selectedType };
                hexTile.Initialize(tileData);

                // Store the tile in the dictionary
                hexTiles[new Vector2Int(q, r)] = hexTile;
                MeshCollider collider = hexTileObject.GetComponent<MeshCollider>();
                NetworkObject networkObject = hexTileObject.GetComponent<NetworkObject>();

                if (networkObject != null)
                {
                    networkObject.Spawn();
                }
                //collider.convex = true;
                tileColliderDict[collider] = hexTile;
                hexTile.originalPos = hexPos;
                positionList.Add(hexTile);
                //hexGrid[q, r] = hexTile;
                count++;

                boardData.boardTiles.Add(tileData);
            }
        }
        var TransformArray = new Transform[count];

        //Sequence sequence = DOTween.Sequence();
        //sequence.SetDelay(0.25f);
        //sequence.onComplete = () =>
        //{
        int i = 0;
        foreach (var tile in hexTiles.Values)
        {
            TransformArray[i] = tile.transform;
            i++;
            //tile.transform.parent = transform;
        }

        m_TransformsAccessArray = new TransformAccessArray(TransformArray);
        //transform.localScale = new Vector3(parentScale, parentScale, parentScale);
        //};

        //sequence.Play();
        Debug.Log("Hex grid generated with " + count + " tiles");
    }

    //void GenerateHexGrid()
    //{
    //    float xOffset = Mathf.Sqrt(3) * HexSize;
    //    float yOffset = 1.5f * HexSize;

    //    for (int q = 0; q < gridWidth; q++)
    //    {
    //        for (int r = 0; r < gridHeight; r++)
    //        {
    //            // Calculate position
    //            float xPos = q * xOffset;
    //            float yPos = r * yOffset;

    //            // Offset rows to create staggered effect
    //            if (r % 2 != 0)
    //            {
    //                xPos += xOffset / 2;
    //            }

    //            // Instantiate the hex tile
    //            GameObject hexTileObject = Instantiate(hexPrefab, transform.position + new Vector3(xPos, 0, yPos), Quaternion.identity, transform);

    //            // Add HexTile component and initialize
    //            HexTile hexTile = hexTileObject.AddComponent<HexTile>();
    //            hexTile.Initialize(new Vector2Int(q, r));

    //            // Store the tile in the dictionary
    //            hexTiles[new Vector2Int(q, r)] = hexTile;
    //            //hexGrid[q, r] = hexTile;
    //        }
    //    }
    //}

    void AssignNeighbors()
    {
        // Define relative positions for neighbors in axial coordinates
        Vector2Int[] neighborOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, 1),  // Top-Left
            new Vector2Int(-1, 1), // Bottom-left
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, -1), // Top-left
            new Vector2Int(1, -1)  // Top-right
        };

        // Assign neighbors to each tile
        foreach (var tile in hexTiles.Values)
        {
            foreach (var offset in neighborOffsets)
            {
                Vector2Int neighborCoord = tile.GridPosition + offset;

                if (hexTiles.TryGetValue(neighborCoord, out IBoardSelectablePosition neighbor))
                {
                    tile.AddNeighbor(neighbor);
                }
            }
        }
    }

    public void HighLightRadius(Vector2Int center, int radius = -1)
    {

    }

    public void ClearHighlights()
    {
        foreach (var tile in selectedTiles)
        {
            tile.ClearHighlight();
            tile.IsSelected = false;
        }
        selectedTiles.Clear();
    }

    public IBoardSelectablePosition GetTile(Vector2Int gridPosition)
    {
        if (hexTiles.TryGetValue(gridPosition, out IBoardSelectablePosition tile))
        {
            return tile;
        }
        return null;
    }

    void OnTileClick(IBoardSelectablePosition tile)
    {
        //Debug.Log($"Clicked Hex: {tile.GridPosition}");
        ClearHighlights();
        tile.SelectNeighbors(movementRange, out selectedTiles);
        tile.PulseSelect(pulseData);
        foreach (var selectedTile in selectedTiles)
        {
            int distance = HexUtility.Distance(tile.GridPosition, selectedTile.GridPosition);
            selectedTile.PulseSelect(pulseData, distance);
        }
        gridShake.Shake();
    }

    public void SetPiecesVisibility(bool state)
    {
        foreach (var tile in hexTiles.Values)
        {
            tile.transform.gameObject.SetActive(state);
        }
    }

    public void ClaimBoardStateOwnership()
    {
        Debug.Log("Claiming board state ownership in hex grid remember to set ownership");
    }

    public void StartGame(VersusGameMode mode, bool isLocal)
    {
        StartGame();
    }

    public void StartGame()
    {
        OnStartGame.Invoke();
        Debug.Log("Should start game now");
    }
    
    public void SelectStartHexTilesForPlayer(int playerIndex)
    {
        if (startTileIndices == null)
        {
            Debug.LogError("Start tile indices not set");
        }
        if (playerIndex < 0 || playerIndex >= startTileIndices.PlayerStartIndices.Length)
        {
            Debug.LogError("Invalid player index");
        }
        for (int i = 0; i < startTileIndices.PlayerStartIndices[playerIndex].indices.Length; i++)
        {
            Vector2Int index = startTileIndices.PlayerStartIndices[playerIndex].indices[i];
            var tile = GetTile(index);
            if (tile != null)
            {
                tile.IsSelected = true;
                selectedTiles.Add(tile);
            }
        }

    }
}

// Utility class for hexagonal grid calculations
public static class HexUtility
{
    public static int Distance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.x + a.y - b.x - b.y)) / 2;
    }

    public static void AddUniqueRange<T>(List<T> list, IEnumerable<T> range)
    {
        foreach (var item in range)
        {
            if (!list.Contains(item)) // Only add if the item isn't already in the list
            {
                list.Add(item);
            }
        }
    }
}

