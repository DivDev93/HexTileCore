using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        // If you are loading scenes without addressables
        var scene = SceneManager.LoadScene("HexGridLevel", new LoadSceneParameters(LoadSceneMode.Single));

        // If you are loading scenes with addressables
        //Addressables.LoadSceneAsync("Greet", activateOnLoad: false).Completed += handle =>
        //{
        //    ReflexSceneManager.PreInstallScene(handle.Result.Scene, builder => builder.AddSingleton("Beautiful"));
        //    handle.Result.ActivateAsync();
        //};
    }
}