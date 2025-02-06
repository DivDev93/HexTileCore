using Reflex.Core;
using Sentences;
using Unity.Netcode;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public TileGameDataScriptableObject tileGameData;
    public StartTileIndicesScriptableObject startTileIndicesScriptableObject;
    public SentenceData sentenceData;
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton<IStaticEvents>(IStaticEvents => new TileEventListeners());
        builder.AddSingleton(tileGameData);
        builder.AddSingleton(startTileIndicesScriptableObject);
        builder.AddSingleton(sentenceData);

        var linePool = new VolumetricLinePool();
        linePool.Initialize();
        builder.AddSingleton(linePool);

        Debug.Log("Hello");
    }
}