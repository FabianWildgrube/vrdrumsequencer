using System;
using UnityEngine;

public class SampleDefinitionSelector : MonoBehaviour
{
    public event Action<SampleDefinition> OnSampleDefinitionSelected;

    public void signalSelection(SampleDefinition sd)
    {
        if (OnSampleDefinitionSelected != null) OnSampleDefinitionSelected(sd);
    }
}
