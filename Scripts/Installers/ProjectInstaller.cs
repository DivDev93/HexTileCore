using Reflex.Core;
using Sentences;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public TileGameDataScriptableObject tileGameData;
    public StartTileIndicesScriptableObject startTileIndicesScriptableObject;
    public SentenceData sentenceData;
    public HexTileFactory hexTileFactory;
    public List<ElementalStrengths> elementalStrengths;
    public string elementalCSVFilePath;

    public void InstallBindings(ContainerBuilder builder)
    {
        Debug.Log("Project bindings installing");
        builder.AddSingleton<IStaticEvents>(IStaticEvents => new TileEventListeners());
        builder.AddSingleton(tileGameData);
        builder.AddSingleton(startTileIndicesScriptableObject);
        builder.AddSingleton(sentenceData);

        Debug.Log("Project bindings halfway installed about to do pool now");
        var linePool = new VolumetricLinePool();
        linePool.Initialize();
        builder.AddSingleton(linePool);
        builder.AddSingleton(hexTileFactory);
        builder.AddSingleton<IStatModifierFactory>(IStatModifierFactory => new StatModifierFactory());

        Debug.Log("Project bindings halfway installed about to do strengths now");
        if (elementalStrengths == null)
            elementalStrengths = ElementDataReader.ReadElementalData(Path.Combine(Application.dataPath, elementalCSVFilePath));
        
        builder.AddSingleton(elementalStrengths);

        Debug.Log("Project bindings Installed");
    }
}