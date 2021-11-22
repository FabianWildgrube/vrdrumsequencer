using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class SampleLibrary
{
    [JsonProperty]
    private Dictionary<string, SampleDefinition> _sampleDefinitions;
    public IReadOnlyCollection<SampleDefinition> SampleDefinitions { get { return _sampleDefinitions.Values; } }

    public event Action<SampleDefinition> OnSampleDefinitionAdded;
    public event Action<SampleDefinition> OnSampleDefinitionUpdated;
    public event Action<SampleDefinition> OnSampleDefinitionRemoved;

    public SampleLibrary()
    {
        _sampleDefinitions = new Dictionary<string, SampleDefinition>();
    }


    public void add(SampleDefinition sd)
    {
        if (!_sampleDefinitions.ContainsKey(sd.name))
        {
            _sampleDefinitions.Add(sd.name, new SampleDefinition(sd)); //copy before adding so changes to that sd must be explicitly committed to the library later on
            if (OnSampleDefinitionAdded != null) OnSampleDefinitionAdded(sd);
        } else
        {
            throw new System.Exception($"SampleDefinition {sd.name} already in SampleLibrary. Could not add it.");
        }

    }

    public void update(SampleDefinition sd)
    {
        if (_sampleDefinitions.ContainsKey(sd.name))
        {
            _sampleDefinitions[sd.name].updateDefinition(sd.vectorValues);
            if (OnSampleDefinitionUpdated != null) OnSampleDefinitionUpdated(sd);
        } else
        {
            throw new System.Exception($"SampleDefinition {sd.name} not found in SampleLibrary. Could not update it.");
        }
    }

    public void remove(SampleDefinition sd)
    {
        if (_sampleDefinitions.ContainsKey(sd.name))
        {
            _sampleDefinitions.Remove(sd.name);
            if (OnSampleDefinitionRemoved != null) OnSampleDefinitionRemoved(sd);
        }
    }

    public bool contains(SampleDefinition sd)
    {
        return _sampleDefinitions.ContainsKey(sd.name);
    }

    public bool contains(string sampleName)
    {
        return _sampleDefinitions.ContainsKey(sampleName);
    }

    public SampleDefinition getNewInstance(string name)
    {
        if (_sampleDefinitions.ContainsKey(name))
        {
            return new SampleDefinition(_sampleDefinitions[name]);
        } else
        {
            throw new System.Exception($"SampleDefinition {name} not found in SampleLibrary. Could not create new Sample from it.");
        }
    }

    public SampleDefinition getNewInstance(SampleDefinition sd)
    {
        return getNewInstance(sd.name);
    }

    public void SaveToFile()
    {
        string libraryJson = JsonConvert.SerializeObject(this, Formatting.Indented);
        //save to user specific directory as to not overwrite the default library
        if (!FileIOUtils.DuplicateSafeWriteToFile(ExperimentManager.instance.generalUserDataDirPath, ExperimentManager.instance.currentParticipantId + "_sampleLibrary", "json", libraryJson))
        {
            Debug.LogError("Could not save SampleLibrary to File!");
        }
    }
}

public class SampleLibraryFactory
{
    private static SampleLibrary _library;

    public static SampleLibrary getLibrary()
    {
        if (_library == null)
        {
            //load default library
            _library = FileIOUtils.createFromJsonFile<SampleLibrary>(System.IO.Path.Combine(GlobalConstants.appDataDirPath, GlobalConstants.SampleLibraryFileName + ".json"));
        }
        return _library;
    }
}
