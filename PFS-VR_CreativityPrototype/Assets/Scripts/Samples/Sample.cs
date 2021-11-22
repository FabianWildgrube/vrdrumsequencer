using System;
using CircularBuffer;
using UnityEngine;

/// <summary>
/// A sample pairs a sampledefinition with an audioclip that contains the sound for the definition.
/// It provides functions to change the definition, which get the clip from the server whenever the definition is changed.
/// 
/// Also implements an undo/redo history for such changes. As soon as a "manual" change happens, the redo history cleared.
/// </summary>
public class Sample
{
    private SampleDefinition _definition;
    public SampleDefinition definition { get { return _definition; } }
    private AudioClip _clip;
    public AudioClip clip { get { return _clip; } }

    private int clipRevisionNr = 0;

    //buffer for the last 50 edits to this sample
    private CircularBuffer<SampleDefinition> undoHistory = new CircularBuffer<SampleDefinition>(50);
    private CircularBuffer<SampleDefinition> redoHistory = new CircularBuffer<SampleDefinition>(50);

    public string name { get { return _definition.name;  } }

    public event Action<AudioClip> OnClipChanged;
    public event Action<string> OnNameChanged;

    public Sample(string name)
    {
        _definition = SampleDefinition.makeRandomSample(name);
        getSampleClipForDefinition();
    }

    public Sample(SampleDefinition definition)
    {
        setNewDefinition(definition);
    }

    public void setNewDefinition(SampleDefinition sd)
    {
        this._definition = sd;
        getSampleClipForDefinition();
    }

    public void update(int idx, float value)
    {
        saveCurrentDefToUndoHistory();
        clearRedoHistory();
        _definition.updateDefinition(idx, value);
        getSampleClipForDefinition();
    }

    public void update(float[] values)
    {
        saveCurrentDefToUndoHistory();
        clearRedoHistory();
        _definition.updateDefinition(values);
        getSampleClipForDefinition();
    }

    public void undoLastUpdate()
    {
        if (!undoHistory.IsEmpty)
        {
            saveCurrentDefToRedoHistory();
            SampleDefinition previousSd = undoHistory.Front();
            undoHistory.PopFront();
            setNewDefinition(previousSd);
        }
    }

    public void redoLastUndoneUpdate()
    {
        if (!redoHistory.IsEmpty)
        {
            saveCurrentDefToUndoHistory();
            SampleDefinition undoneSd = redoHistory.Front();
            redoHistory.PopFront();
            setNewDefinition(undoneSd);
        }
    }

    private void clearRedoHistory()
    {
        while (!redoHistory.IsEmpty)
        {
            redoHistory.PopFront();
        }
    }

    public float getValue(int index)
    {
        return _definition.vectorValues[index];
    }

    public void setName(string newName)
    {
        definition.name = newName;
        if (OnNameChanged != null) OnNameChanged(newName);
    }

    private void saveCurrentDefToUndoHistory()
    {
        //save a copy of the current definition
        undoHistory.PushFront(new SampleDefinition(_definition));
    }

    private void saveCurrentDefToRedoHistory()
    {
        //save a copy of the current definition
        redoHistory.PushFront(new SampleDefinition(_definition));
    }

    private void getSampleClipForDefinition()
    {
        SampleServer.instance.SendSampleRequest(_definition, this.updateClip);
    }

    private void updateClip(float[] rawSoundData)
    {
        try
        {
            Debug.Log("Creating clip from raw data (" + rawSoundData.Length + " bytes)");
            _clip = AudioClip.Create(_definition.name + "_v" + clipRevisionNr++, rawSoundData.Length, 1, 16000, false);
            _clip.SetData(rawSoundData, 0);
            _clip.LoadAudioData();
            Debug.Log("Clip was constructed: " + _clip.name);
            if (OnClipChanged != null)
            {
                OnClipChanged(_clip);
            }
        } catch(Exception e)
        {
            Debug.Log("Exception in updateClip: " + e.Message);
        }
        
    }

}
