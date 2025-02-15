using UnityEngine;

public class TileGameData
{
    public float duration = 0.25f;
    public float delay = 0.1f;
    public float height = 0.3f;
}

[CreateAssetMenu(fileName = "TileGameData", menuName = "HexTileCore/TileGameData", order = 1)]
public class TileGameDataScriptableObject : ScriptableObject
{
    [SerializeField]
    float yOffset = 1f;
    public float YOffset => yOffset * parentScale;
    public float parentScale = 0.025f; // Scale of the game board
    public PulseData pulseData = new PulseData { duration = 0.25f, delay = 0.1f, height = 0.3f };
    [SerializeField]
    float placedCardScale = 0.18f; // Scale of the placed card
    public float PlacedCardScale => placedCardScale * parentScale;
    public float sphereCastSize = 0.017f;
    public float selectAnimationDuration = 0.25f;
    public float cardSelectionPlaneHeight = 0.2f;
    public float cardPlacedDisplacement = 0.01f;
    public float cardPulsePower = 0.00001f;

}
