using System;
using UnityEngine;

public enum BeatSubdivision
{
    Viertel = 1,
    Achtel = 2,
    Triole = 3,
    Sechzehntel = 4,
    Fünftole = 5,
    Sextole = 6,
    Septole = 7,
    Zweiunddreißigstel = 8,
}

public class BPMManager : MonoBehaviour
{
    public static BPMManager instance;

    public float bpm = 60f;
    public int beatsPerBar = 4; //signature High
    public int beatType = 4; //signature low
    public BeatSubdivision beatSubdivisions = BeatSubdivision.Triole;

    public event Action OnAnyParameterChangedFromCode;

    public float secondsPerBeat => 60.0f / bpm * (4.0f / (float)beatType); //bpm signify quarters per minute -> time of a beat needs to be relative to the time a quarter takes at the set bpm
    public float secondsPerSubBeat => secondsPerBeat / (float)((int)beatSubdivisions);
    public float secondsPerBar => secondsPerBeat * beatsPerBar;

    public float distancePerBeat = 0.2f;
    public float distancePerSubBeat => distancePerBeat / (float)((int)beatSubdivisions);
    public float distancePerBar => distancePerBeat * beatsPerBar;

    public float subbeatSnappingDistance => distancePerSubBeat * 0.45f; // < 0.5 to prevent overlap of snap "zones"

    public event Action<float> OnBPMChange; //pass the factor of change
    public event Action<int> OnBeatsPerBarChange; //pass the factor of change
    public event Action<int> OnBeatTypeChange; //pass the factor of change
    public event Action<BeatSubdivision> OnBeatSubdivisionsChange; //pass the factor of change
    public event Action<float> OnDistancePerBeatChange; //pass the factor of change

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public float barLength { get { return beatsPerBar * distancePerBeat; } }

    public float distanceToTime(float distance)
    {
        return (distance / distancePerBeat) * secondsPerBeat;
    }

    public float timeToDistance(float time)
    {
        return (time / secondsPerBeat) * distancePerBeat;
    }

    public float closestSubBeatTime(float timeInLoop)
    {
        int subBeatCtr = 0;
        float timeLeft = timeInLoop;
        while (timeLeft > secondsPerSubBeat)
        {
            timeLeft -= secondsPerSubBeat;
            subBeatCtr++;
        }

        int closestSubBeatNr = timeLeft > secondsPerSubBeat * 0.5f ? subBeatCtr + 1 : subBeatCtr;

        return closestSubBeatNr * secondsPerSubBeat;
    }

    public void setBPM(float newBPM, bool fromUI = true)
    {
        bpm = newBPM;
        if (OnBPMChange != null) OnBPMChange(newBPM);
        signalChange(fromUI);
    }

    public void setBeatsPerBar(int newBeatsPerBar, bool fromUI = true)
    {
        beatsPerBar = newBeatsPerBar;
        if (OnBeatsPerBarChange != null) OnBeatsPerBarChange(newBeatsPerBar);
        signalChange(fromUI);
    }

    public void setBeatType(int newBeatType, bool fromUI = true)
    {
        beatType = newBeatType;
        if (OnBeatTypeChange != null) OnBeatTypeChange(newBeatType);
        signalChange(fromUI);
    }

    public void setDistancePerBeat(float newDistancePerBeat, bool fromUI = true)
    {
        float changeFactor = newDistancePerBeat / distancePerBeat;
        distancePerBeat = newDistancePerBeat;
        if (OnDistancePerBeatChange != null) OnDistancePerBeatChange(changeFactor);
        signalChange(fromUI);
    }

    public void setBeatSubdivisions(BeatSubdivision newSubdivision, bool fromUI = true)
    {
        beatSubdivisions = newSubdivision;
        if (OnBeatSubdivisionsChange != null) OnBeatSubdivisionsChange(newSubdivision);
        signalChange(fromUI);
    }

    private void signalChange(bool changesCameFromUI)
    {
        if (!changesCameFromUI && OnAnyParameterChangedFromCode != null) OnAnyParameterChangedFromCode();
    }
}
