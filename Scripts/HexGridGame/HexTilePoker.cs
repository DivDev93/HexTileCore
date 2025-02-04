using UnityEngine;

public class HexTilePoker : MonoBehaviour
{
    public HexTile currentHexTile = null;
    public float durationToClick = 0.35f;
    float currentHexHoverDuration = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if(HexGridManager.tileColliderDict.TryGetValue(other, out HexTile hexTile))
        {
            if (currentHexTile != null && currentHexTile != hexTile)
                currentHexTile.OnHoverExit();

            currentHexHoverDuration = 0f;
            currentHexTile = hexTile;
            hexTile.OnHoverEnter();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (HexGridManager.tileColliderDict.TryGetValue(other, out HexTile hexTile))
        {
            if (currentHexTile == hexTile)
            {
                currentHexHoverDuration += Time.deltaTime;
                if (currentHexHoverDuration > durationToClick)
                {
                    currentHexTile.OnTileClick();
                    currentHexHoverDuration = 0f;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (HexGridManager.tileColliderDict.TryGetValue(other, out HexTile hexTile))
        {
            if (currentHexTile != null)
            {
                currentHexTile.OnHoverExit();
                if (hexTile == currentHexTile)
                    currentHexTile = null;

                currentHexHoverDuration = 0f;
            }
            else
                hexTile.OnHoverExit();
        }
    }
}
