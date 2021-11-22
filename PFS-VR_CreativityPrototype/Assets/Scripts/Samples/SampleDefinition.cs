using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;

[System.Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class SampleDefinition
{
    [JsonProperty]
    private float[] _vectorValues;
    public float[] vectorValues
    {
        get
        {
            return _vectorValues;
        }
    }

    [JsonProperty]
    public string name;

    [JsonProperty]
    public System.DateTime creationDate;

    public SampleDefinition()
    {
        _vectorValues = GlobalConstants.defaultVector;
        name = "defaultSample";
        creationDate = System.DateTime.Now;
    }

    public SampleDefinition(float[] values, string name)
    {
        if (values.Length != GlobalConstants.NR_OF_VECTOR_VALUES)
        {
            Debug.LogWarning("Trying to initialize a SampleDefinition with not exactly 100 vectorValues! Using default instead");
            _vectorValues = GlobalConstants.defaultVector;
        }
        _vectorValues = values;
        this.name = name;
        creationDate = System.DateTime.Now;
    }

    public SampleDefinition(SampleDefinition other)
    {
        _vectorValues = new float[other.vectorValues.Length];
        Array.Copy(other.vectorValues, _vectorValues, other.vectorValues.Length);
        name = other.name;
        creationDate = other.creationDate;
    }

    static public SampleDefinition makeRandomSample(string name)
    {
        return new SampleDefinition(Utils.getRandomSampleVectorValues(), name);
    }

    static public SampleDefinition makeAllOnesSample(string name)
    {
        float[] onesVector = Enumerable
            .Repeat(0, GlobalConstants.NR_OF_VECTOR_VALUES)
            .Select(i => 0.0f)
            .ToArray();

        return new SampleDefinition(onesVector, name);
    }

    public void updateDefinition(int idx, float value)
    {
        if (idx >= 0 && idx < GlobalConstants.NR_OF_VECTOR_VALUES)
        {
            _vectorValues[idx] = value;
            creationDate = System.DateTime.Now;
        }
    }

    public void updateDefinition(float[] newValues)
    {
        if (newValues.Length != GlobalConstants.NR_OF_VECTOR_VALUES)
        {
            Debug.LogWarning("Trying to updateDefinition with not exactly the number of values! Ignoring.");
            return;
        }

        Array.Copy(newValues, _vectorValues, newValues.Length);
        creationDate = System.DateTime.Now;
    }
}
