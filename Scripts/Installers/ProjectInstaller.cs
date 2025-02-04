using Reflex.Core;
using Unity.Netcode;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public TileGameDataScriptableObject tileGameData;
    public StartTileIndicesScriptableObject startTileIndicesScriptableObject;
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton(tileGameData);
        builder.AddSingleton(startTileIndicesScriptableObject);
        Debug.Log("Hello");
    }
}