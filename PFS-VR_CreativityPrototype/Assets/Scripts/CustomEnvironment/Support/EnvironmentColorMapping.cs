using UnityEngine;

[System.Serializable]
public class AccentLightColorMapping
{
    public AccentLightColor colorType;
    public Color color;
    public Material emissiveMaterial;
}

[System.Serializable]
public class AccentObjectColorMapping
{
    public AccentObjectColor colorType;
    public Gradient colors;
    public Material material;
}

[System.Serializable]
public class AccentObjectColorSkyboxMapping
{
    public AccentObjectColor colorType;
    public Material skybox;
}

[CreateAssetMenu(fileName = "Data", menuName = "PFS-VR/EnvironmentColorMapping", order = 1)]
public class EnvironmentColorMapping : ScriptableObject
{
    public Gradient mainLightColorTempGradient;
    public AccentLightColorMapping[] accentLightColors = new AccentLightColorMapping[System.Enum.GetNames(typeof(AccentLightColor)).Length];
    public AccentObjectColorMapping[] colorfolObjectsColors = new AccentObjectColorMapping[System.Enum.GetNames(typeof(AccentObjectColor)).Length];
}
