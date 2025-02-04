using NUnit.Framework;
using System;
using UnityEngine;

[Serializable]
public class StartTileIndices
{
    public Vector2Int[] indices;
}

[CreateAssetMenu(fileName = "ScriptableObjects/StartTileIndices", menuName = "HexGridGame/StartTileIndices")]
public class StartTileIndicesScriptableObject : ScriptableObject
{
    public StartTileIndices[] PlayerStartIndices;
}
