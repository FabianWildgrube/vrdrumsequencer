using UnityEngine;

public class AccentLight : MonoBehaviour
{
    public Light lightObj;
    public GameObject representation;

    private Renderer emissiveRenderer;

    private float maxLightIntensity;
    private float maxEmissiveMaterialIntensity;
    private Color emissiveBaseColor;
        
    private const byte k_MaxByteForOverexposedColor = 191; //internal Unity const

    private void Awake()
    {
        maxLightIntensity = lightObj.intensity;

        if (representation != null)
        {
            emissiveRenderer = representation.GetComponent<Renderer>();
        }

        updateEmissiveMaterialInfos();
    }

    private void updateEmissiveMaterialInfos()
    {
        if (emissiveRenderer != null)
        {
            Color _emissionColor = emissiveRenderer.material.GetColor("_EmissionColor");
            var maxColorComponent = _emissionColor.maxColorComponent;
            var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
            maxEmissiveMaterialIntensity = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);

            float exposure = Mathf.Pow(maxEmissiveMaterialIntensity, 2f);
            emissiveBaseColor = new Color(_emissionColor.r /exposure, _emissionColor.g / exposure, _emissionColor.b / exposure);
        }
    }

    public void changeColor(AccentLightColorMapping newColorMapping)
    {
        lightObj.color = newColorMapping.color;
        if (emissiveRenderer != null)
        {
            emissiveRenderer.material = newColorMapping.emissiveMaterial;
            updateEmissiveMaterialInfos();
        }
    }

    public void changeIntensity(float newNormalizedIntensity)
    {
        lightObj.intensity = maxLightIntensity * newNormalizedIntensity;

        if (emissiveRenderer != null)
        {
            float exposure = Mathf.Pow(newNormalizedIntensity * maxEmissiveMaterialIntensity, 2.0f);
            Color currentEmissionColor = emissiveRenderer.material.GetColor("_EmissionColor");
            Color newEmissionColor = new Color(emissiveBaseColor.r * exposure, emissiveBaseColor.g * exposure, emissiveBaseColor.b * exposure, currentEmissionColor.a);

            emissiveRenderer.material.SetColor("_EmissionColor", newEmissionColor);
        }
    }
}
