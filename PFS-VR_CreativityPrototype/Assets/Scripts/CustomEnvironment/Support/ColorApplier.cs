using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorApplier : MonoBehaviour
{
    public Renderer[] colorRenderers;

    public void apply(Color color)
    {
        foreach (var renderer in colorRenderers)
        {
            renderer.material.color = color;
        }
    }
}
