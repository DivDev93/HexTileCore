using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfigEndpoints", menuName = "Scriptable Objects/ServerConfigEndpoints")]
public class ServerConfigEndpoints : ScriptableObject
{
    public string speechToTextEndpoint;
    public string textToImageEndpoint;
    public string imageToModelEndpoint;
    public string openAIEndpoint;
    public string huggingFaceToken;
    public string openAIKey;
}
