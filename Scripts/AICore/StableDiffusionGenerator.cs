using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;  // or use System.Text.Json if you prefer
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using System.Collections.Generic;
using Reflex.Attributes;
using GLTFast.Schema;
using Sentences;

public struct ImageData : INetworkSerializable
{
    public byte[] data;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref data);
    }
}

public class StableDiffusionGenerator : NetworkBehaviour
{
    [Inject]
    public SentenceGenerator sentenceGenerator;

    public bool generateOnStart = true;
    public Shader shader;

    [System.Serializable]
    public class InferenceRequest
    {
        public string inputs;
    }

    public List<WordData> words;
    public string prompt = "A beautiful sunset over the city";
    Texture2D currentTexture;
    public TMP_InputField inputField;
    static ServerConfigEndpoints endpoints = null;
    public bool validate = false;
    public bool isTextureReady = false;

    public void SetPrompt(string newPrompt)
    {
        prompt = newPrompt; 

    }

    void SetMainMaterialTexture(ref Texture2D texture)
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.material.mainTexture = texture;
            isTextureReady = true;
        }
    }

    private void OnValidate()
    {
        if (validate)
        {
            prompt = sentenceGenerator.GetRandomizedPrompt(out words);
            validate = false;
        }
    }

    private void Awake()
    {
        if (endpoints == null)
            endpoints = Resources.Load<ServerConfigEndpoints>("EndpointsConfig");

        // Call this from somewhere to begin the image generation
        if (generateOnStart)
        {
            if (IsServer || !NetworkManager.Singleton.IsListening)
            {
                prompt = sentenceGenerator.GetRandomizedPrompt(out words);
                StartImageGeneration();
            }          
        }

        if(inputField != null)
            inputField.onValueChanged.AddListener(SetPrompt);
    }

    // Call this from somewhere to begin the image generation
    public async void StartImageGeneration()
    {
        await GenerateImageFromHuggingFace(prompt).ToUniTask();
        //StartCoroutine(GenerateImageFromHuggingFace(prompt));
    }

    [ClientRpc]
    private void RpcDistributeTextureClientRpc(byte[] textureData)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureData);

        SetMainMaterialTexture(ref texture);
    }

    List<byte> textureBuffer = new List<byte>();

    [ClientRpc]
    void RpcSendTextureChunkClientRpc(byte[] chunk, bool isLastChunk)
    {
        textureBuffer.AddRange(chunk);

        if (isLastChunk)
        {
            // Reconstruct the texture when the last chunk is received
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(textureBuffer.ToArray());
            textureBuffer.Clear();

            // Apply the texture
            SetMainMaterialTexture(ref texture);
        }
    }

    public void RPCSendImageDataSerializableRpc(byte[] imageBytes)
    {
        ImageData imageData = new ImageData { data = imageBytes };
        ReceiveImageDataClientRpc(imageData);
    }

    [ClientRpc]
    public void ReceiveImageDataClientRpc(ImageData imageData)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData.data);
        SetMainMaterialTexture(ref texture);
    }

    public void SendTextureInChunks(byte[] textureData)
    {
        int chunkSize = 1024; // 1KB per chunk
        int totalChunks = Mathf.CeilToInt((float)textureData.Length / chunkSize);

        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * chunkSize;
            int size = Mathf.Min(chunkSize, textureData.Length - offset);
            byte[] chunk = new byte[size];
            System.Array.Copy(textureData, offset, chunk, 0, size);

            bool isLastChunk = (i == totalChunks - 1);
            RpcSendTextureChunkClientRpc(chunk, isLastChunk);
        }
    }

    private IEnumerator GenerateImageFromHuggingFace(string prompt)
    {
        // Model endpoint from the JavaScript example
        //string endpoint =  "https://api-inference.huggingface.co/models/stabilityai/stable-diffusion-3.5-large-turbo";//"https://api-inference.huggingface.co/models/Jovie/Midjourney";//

        string promptPreface = "These images will be used to generate 3d models of kaijus. Generate a kaiju with it's whole body entirely in frame with the description: ";
        // Prepare the request body
        var requestData = new InferenceRequest { inputs = promptPreface + prompt };
        // Convert the request to JSON
        string jsonData = JsonConvert.SerializeObject(requestData);

        // Create the UnityWebRequest
        using (UnityWebRequest webRequest = new UnityWebRequest(endpoints.textToImageEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + endpoints.huggingFaceToken);

            // Send the request
            yield return webRequest.SendWebRequest();

            // Check for network or server errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // The response is a binary image (PNG/JPEG)
                byte[] imageData = webRequest.downloadHandler.data;

                // Convert the raw bytes into a Texture2D
                currentTexture = new Texture2D(2, 2);
                currentTexture.LoadImage(imageData);
                SendTextureInChunks(imageData);
                //RPCSendImageDataSerializableRpc(imageData);

                // Example: apply the texture to this GameObject's material
                SetMainMaterialTexture(ref currentTexture);

                // If using UI, you could assign tex to an Image component's sprite, for example.
            }
            else
            {
                Debug.LogError("Image generation request failed: " + webRequest.error);
            }
        }
    }

    public async void CreateModelFromCurrentTexture()
    {
        Debug.Log("Creating model from current texture...");
        await TrellisClient.UploadImageData(currentTexture.EncodeToPNG(), transform, shader).ToUniTask();
        //StartCoroutine(TrellisClient.UploadImageData(currentTexture.EncodeToPNG(), transform, shader));
    }

    //ongui button to trigger the image generation
    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 130, 150, 50), "Generate Image"))
    //    {
    //        StartImageGeneration();
    //    }

    //    if (GUI.Button(new Rect(10, 190, 150, 50), "Create Model"))
    //    {
    //        CreateModelFromCurrentTexture();
    //    }
    //}
}
