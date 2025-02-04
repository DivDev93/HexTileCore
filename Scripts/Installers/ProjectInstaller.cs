using Reflex.Core;
using Unity.Netcode;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public NetworkManager networkManager;
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton(networkManager);
        Debug.Log("Hello");
    }
}