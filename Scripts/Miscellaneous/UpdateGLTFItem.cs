using System.Collections;
using UnityEngine;

public class UpdateGLTFItem : MonoBehaviour
{
    public Renderer modelRenderer;
    public Shader shader;
    public Texture mainTex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        while(!modelRenderer)
        {
            yield return null;
            modelRenderer = GetComponentInChildren<Renderer>();
        }

        mainTex = modelRenderer.material.mainTexture;
        modelRenderer.material.shader = shader;
        modelRenderer.material.mainTexture = mainTex;
    }

    public Material CreateMaterial(Material originalMaterial)
    {
        Material newMaterial = new Material(shader);
        newMaterial.CopyPropertiesFromMaterial(originalMaterial);
        return newMaterial;
    }

    public void SetCutOffValue(float value)
    {
        // Adjust the cutoff value based on the mesh's Y position
        float adjustedValue = value + transform.position.y;
        modelRenderer.material.SetFloat("_CutoffY", adjustedValue);
    }
}
