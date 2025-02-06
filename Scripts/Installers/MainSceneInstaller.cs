using UnityEngine;
using Reflex;
using Reflex.Core;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Netcode;

public class MainSceneInstaller : MonoBehaviour, IInstaller
{
    public TransformShaker cameraScreenShake;
    public HexGridManager hexGridManager;
    public InterfaceReference<IGameManager> gameManager;
    public SentenceGenerator sentenceGenerator;
    public InterfaceReference<IGameUI> gameUIref;

    //IEnumerator Start()
    //{
    //    while (!SceneManager.GetActiveScene().isLoaded)
    //    {
    //        yield return null;
    //    }

    //    Debug.Log("Scene merging active scene is " + SceneManager.GetActiveScene().name + " loaded count is " + SceneManager.loadedSceneCount);
    //    SceneManager.MergeScenes(SceneManager.GetActiveScene(), gameObject.scene);
    //}
    //IEnumerator Start()
    //{
    //    //Sequence sequence = DOTween.Sequence();
    //    //sequence.AppendInterval()

    //    //await sequence.AsyncWaitForCompletion();

    //    yield return null;


    //}

    public void InstallBindings(ContainerBuilder builder)
    {
        // Add Scene bindings here
        IShakeable shakeable = cameraScreenShake;
        builder.AddSingleton<IShakeable>(IShakeable => shakeable);
        builder.AddSingleton<IGameUI>(IGameUI => gameUIref.Value);
        builder.AddSingleton<IGameBoard>(IGameBoard => hexGridManager);
        builder.AddSingleton(hexGridManager);
        builder.AddSingleton<IGameManager>(IGameManager => gameManager.Value);
        builder.AddSingleton(sentenceGenerator);
        Debug.Log("MainSceneInstaller bindings installed");
    }
}
