using UnityEngine;


[RequireComponent(typeof(Renderer))]
public class SimpleColoredMeshTrackIcon : MonoBehaviour, ITrackIconVisualisation
{
    public float emissiveIntensity = 0.95f;

    public void setColor(Color c)
    {
        var r = GetComponent<Renderer>();
        r.material.EnableKeyword("_EMISSION");
        r.material.color = c;
        r.material.SetColor("_EmissionColor", new Color(c.r * emissiveIntensity, c.g * emissiveIntensity, c.b * emissiveIntensity, 1f));
    }
}
