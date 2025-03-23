using System;
using System.Collections.Generic;
using UnityEngine;

//Ensure that the enum values are the same as the index of the mesh in the hexMeshPrefabs array
[Serializable]
public enum EElementType
{
    FIRE = 0,
    METAL = 1,
    COSMIC = 2,
    ELECTRIC = 3,
    EARTH = 4,
    ICE = 5,
    WIND = 6,
}

public class HexTileFactory : MonoBehaviour
{
    public Mesh[] hexMeshPrefabs;
    public EElementType[] tileTypeArray = { EElementType.FIRE,
        EElementType.METAL,
        EElementType.COSMIC,
        EElementType.ELECTRIC,
        EElementType.EARTH,
        EElementType.ICE,
        EElementType.WIND
    };
    public void ShuffleTypes()
    {
        for (int i = 0; i < tileTypeArray.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, tileTypeArray.Length);
            EElementType temp = tileTypeArray[i];
            tileTypeArray[i] = tileTypeArray[randomIndex];
            tileTypeArray[randomIndex] = temp;
        }
    }

    public Mesh GetMeshForTileType(EElementType tileType)
    {
        return hexMeshPrefabs[(int)tileType];
    }
}
