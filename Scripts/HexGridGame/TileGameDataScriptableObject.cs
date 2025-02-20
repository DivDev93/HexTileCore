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
    const int spacing = 32;

    [Header("Game Board Settings")]
    [Space(spacing)]
    public float parentScale = 0.025f; // Scale of the game board

    [Header("Card Animation Settings")]
    [Space(spacing)]
    public PulseData pulseData = new PulseData { duration = 0.25f, delay = 0.1f, height = 0.3f };
    public float selectAnimationDuration = 0.25f;
    public float cardPulsePower = 0.00001f;

    [Header("Card Position Settings")]
    [Space(spacing)]
    [SerializeField] float yOffset = 1f;
    public float YOffset => yOffset * parentScale;
    [SerializeField] float placedCardScale = 0.18f; // Scale of the placed card
    public float PlacedCardScale => placedCardScale * parentScale;
    public float sphereCastSize = 0.017f;
    public float cardSelectionPlaneHeight = 0.2f;
    public float cardPlacedDisplacement = 0.01f;

    [Header("Volumetric Line Renderer Settings")]
    [Space(spacing)]
    [SerializeField] float m_volumetricStartOffset = 0.1f;
    public float VolumetricStartOffset => m_volumetricStartOffset * parentScale;
}
