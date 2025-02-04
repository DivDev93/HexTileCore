using GLTFast;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static StableDiffusionGenerator;

[System.Serializable]
public class GLBRequest
{
    public byte[] imageData;    
}

public class TrellisClient : MonoBehaviour
{
    //private static string serverUrl = "http://20.119.99.198:8000/generate_model/";

    private static ServerConfigEndpoints _endpoints;
    public static ServerConfigEndpoints endpoints
    {
        get
        {
            if (_endpoints == null)
            {
                _endpoints = Resources.Load<ServerConfigEndpoints>("EndpointsConfig");
            }
            return _endpoints;
        }
    }

    public static IEnumerator UploadImage(string imagePath)
    {
        byte[] imageData = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData);

        using (UnityWebRequest www = UnityWebRequest.Post(endpoints.imageToModelEndpoint, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] modelData = www.downloadHandler.data;
                string modelPath = Path.Combine(Application.persistentDataPath, "output_model.glb");
                File.WriteAllBytes(modelPath, modelData);
                Debug.Log("Model saved to: " + modelPath);
                // Implement further logic to load and display the model in Unity
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
    }

    public static IEnumerator UploadImageData(byte[] _imageData, Transform spawningObject, Shader shader)
    {
        //Debug.Log("Uploading image data...");
        //// Create the UnityWebRequest
        //using (UnityWebRequest webRequest = new UnityWebRequest(serverUrl, "POST"))
        //{
        //    //byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        //    webRequest.uploadHandler = new UploadHandlerRaw(_imageData);
        //    webRequest.downloadHandler = new DownloadHandlerBuffer();
        //    //webRequest.SetRequestHeader("Content-Type", "application/json");
        //    //webRequest.SetRequestHeader("Authorization", "Bearer " + token);

        //    Debug.Log("Sending request...");
        //    // Send the request
        //    yield return webRequest.SendWebRequest();

        //    // Check for network or server errors
        //    if (webRequest.result == UnityWebRequest.Result.Success)
        //    {
        //        byte[] modelData = webRequest.downloadHandler.data;
        //        string modelPath = Path.Combine(Application.dataPath, "output_model_" + Time.time + ".glb");
        //        File.WriteAllBytes(modelPath, modelData);
        //        Debug.Log("Model saved to: " + modelPath);

        //        // If using UI, you could assign tex to an Image component's sprite, for example.
        //    }
        //    else
        //    {
        //        Debug.LogError("Model generation request failed: " + webRequest.error);
        //    }
        //}

        Debug.Log("Uploading image data...");

        // Create a WWWForm and add the file
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", _imageData, "image.png", "image/png");

        // Create the UnityWebRequest
        using (UnityWebRequest webRequest = UnityWebRequest.Post(endpoints.imageToModelEndpoint, form))
        {
            Debug.Log("Sending request...");
            // Send the request
            yield return webRequest.SendWebRequest();

            // Check for network or server errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Handle the response (e.g., model data)
                byte[] modelData = webRequest.downloadHandler.data;
                string modelName = "output_model_" + Time.timeAsRational.Count + ".glb";
                string modelPath = Path.Combine(Application.persistentDataPath, modelName);
                File.WriteAllBytes(modelPath, modelData);
                //instantiate GLTFObject from Resources
                GltfAsset gltfObject = Resources.Load<GltfAsset>("GLTFObject");
                gltfObject.Url = modelPath;

                GameObject gltfObjectInstance = Instantiate(gltfObject.gameObject);
                gltfObjectInstance.transform.position = spawningObject.transform.position + Vector3.left * 1f;
                gltfObjectInstance.transform.LookAt(Camera.main.transform);
                
                //Renderer renderer = gltfObjectInstance.GetComponentInChildren<Renderer>();
                yield return SetShader(gltfObjectInstance, shader);

                Debug.Log("Model saved to: " + modelPath);
            }
            else
            {
                // Log the error
                Debug.LogError("Model generation request failed: " + webRequest.error);
                Debug.LogError("Server Response: " + webRequest.downloadHandler.text);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    static IEnumerator SetShader(GameObject gltfObj, Shader shader)
    {
        Renderer modelRenderer = null;
        while (modelRenderer == null)
        {
            yield return null;
            modelRenderer = gltfObj.GetComponentInChildren<Renderer>();
        }
        var mainTex = modelRenderer.material.mainTexture;
        modelRenderer.material.shader = shader;
        modelRenderer.material.mainTexture = mainTex;
    }
}
