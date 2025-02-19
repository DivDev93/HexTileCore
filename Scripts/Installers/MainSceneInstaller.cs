using UnityEngine;
using Reflex;
using Reflex.Core;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using System;

[Serializable]
public class ScreenSpaceInputs
{
    public ScreenSpaceRayPoseDriver screenSpaceRayPoseDriver;
    public ScreenSpaceRotateInput screenSpaceRotateInput;
    public ScreenSpaceSelectInput screenSpaceSelectInput;
    public ScreenSpacePinchScaleInput screenSpacePinchScaleInput;
}

public class MainSceneInstaller : MonoBehaviour, IInstaller
{
    public TransformShaker cameraScreenShake;
    public HexGridManager hexGridManager;
    public InterfaceReference<ICardSpawner> cardSpawner;
    public InterfaceReference<IGameManager> gameManager;
    public SentenceGenerator sentenceGenerator;
    public InterfaceReference<IGameUI> gameUIref;
    public ScreenSpaceInputs screenSpaceInputs;
    public CardInfoUI cardInfoUI;

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
        Debug.Log("MainSceneInstaller bindings installing");
        // Add Scene bindings here
        IShakeable shakeable = cameraScreenShake;
        builder.AddSingleton<ICardSpawner>(ICardSpawner => cardSpawner.Value);
        builder.AddSingleton<IShakeable>(IShakeable => shakeable);
        builder.AddSingleton<IGameUI>(IGameUI => gameUIref.Value);
        builder.AddSingleton<IGameBoard>(IGameBoard => hexGridManager);
        Debug.Log("MainSceneInstaller bindings halfway installed");
        builder.AddSingleton(hexGridManager);
        builder.AddSingleton<IGameManager>(IGameManager => gameManager.Value);
        builder.AddSingleton(sentenceGenerator);
        builder.AddSingleton(screenSpaceInputs);
        builder.AddSingleton(cardInfoUI);
        Debug.Log("MainSceneInstaller bindings installed");
    }
}
