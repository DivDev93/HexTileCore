using Reflex.Attributes;
using UnityEngine;

public class HexTilePoker : MonoBehaviour
{
    [Inject]
    IGameBoard gameBoard;

    public IBoardSelectablePosition m_currentHexTile = null;
    public float durationToClick = 0.35f;
    float currentHexHoverDuration = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if(gameBoard.tileColliderDict.TryGetValue(other, out IBoardSelectablePosition boardPosition))
        {
            HexTile currentHexTile = m_currentHexTile as HexTile;
            HexTile triggerHextile = boardPosition as HexTile;
            if (currentHexTile != null && currentHexTile != triggerHextile)
                currentHexTile.OnHoverExit();

            currentHexHoverDuration = 0f;
            currentHexTile = triggerHextile;
            triggerHextile.OnHoverEnter();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (gameBoard.tileColliderDict.TryGetValue(other, out IBoardSelectablePosition boardPosition))
        {
            HexTile currentHexTile = m_currentHexTile as HexTile;
            HexTile triggerHextile = boardPosition as HexTile;

            if (currentHexTile == triggerHextile)
            {
                currentHexHoverDuration += Time.deltaTime;
                if (currentHexHoverDuration > durationToClick)
                {
                    currentHexTile.OnSelectionClick();
                    currentHexHoverDuration = 0f;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (gameBoard.tileColliderDict.TryGetValue(other, out IBoardSelectablePosition boardPosition))
        {
            HexTile currentHexTile = m_currentHexTile as HexTile;
            HexTile triggerHextile = boardPosition as HexTile;

            if (currentHexTile != null)
            {
                currentHexTile.OnHoverExit();
                if (triggerHextile == currentHexTile)
                    currentHexTile = null;

                currentHexHoverDuration = 0f;
            }
            else
                triggerHextile.OnHoverExit();
        }
    }
}
