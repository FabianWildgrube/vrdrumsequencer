using System.Collections.Generic;

[System.Serializable]
public class NoteExportData
{
    public double triggerTime;
    public float posOnTrackAxis; //between 0.0-1.0 represents where in the loop's time it is
}

public struct TrackColor
{
    public float r;
    public float g;
    public float b;
}

[System.Serializable]
public class TrackExportData
{
    public string trackName;
    public TrackColor color;
    public SampleDefinition sampleDefinition;
    public float trackVolume;

    public bool isMuted;
    public bool isSolo;
    public bool anotherIsSolo;

    public List<NoteExportData> notes;
}

[System.Serializable]
public class LoopExportData
{
    public string participantId;
    public string conditionName;

    public string loopName;
    public System.DateTime exportDate;

    public float bpm;
    public int timeSignatureHi;
    public int timeSignatureLo;
    public float metersPerBeat;

    public int durationInBars;

    public string songName;
    public float songVolume;

    public List<TrackExportData> tracks;
}