using UnityEngine;

public class BPMSettingsChangeUI : MonoBehaviour
{
    public ValueSliderCtrl bpmslider;
    public ValueStepCtrl beatSignatureHiStepCtrl;
    public ValueStepCtrl beatSignatureLoStepCtrl;
    public ValueSliderCtrl distancePerBeatSlider;

    private void Start()
    {
        updateUiToNewValues();
        BPMManager.instance.OnAnyParameterChangedFromCode += this.updateUiToNewValues;
    }

    private void OnDestroy()
    {
        BPMManager.instance.OnAnyParameterChangedFromCode -= this.updateUiToNewValues;
    }

    private void updateUiToNewValues()
    {
        bpmslider.setValue((int)Mathf.Round(BPMManager.instance.bpm));
        beatSignatureHiStepCtrl.setValueSilently(BPMManager.instance.beatsPerBar);
        beatSignatureLoStepCtrl.setValueSilently(BPMManager.instance.beatType);
        distancePerBeatSlider.setValue(BPMManager.instance.distancePerBeat);
    }

    public void handleBPMChange(float newBPMValue)
    {
        BPMManager.instance.setBPM(newBPMValue);
    }

    public void handleBeatSignatureHighChange(int newSignatureHi)
    {
        BPMManager.instance.setBeatsPerBar(newSignatureHi);
        UsageLogger.log(UserAction.BPM_SIGNATURE_HI_CHANGED); //not a slider -> this function is truly only called when the user triggers it
    }

    public void handleBeatSignatureLowChange(int newSignatureLo)
    {
        BPMManager.instance.setBeatType(newSignatureLo);
        UsageLogger.log(UserAction.BPM_SIGNATURE_LOW_CHANGED); //not a slider -> this function is truly only called when the user triggers it
    }

    public void handleBeatSubSignatureChange(BeatSubdivision newSubdivision)
    {
        BPMManager.instance.setBeatSubdivisions(newSubdivision);
        UsageLogger.log(UserAction.BPM_SUBBEAT_SIGNATURE_CHANGED); //not a slider -> this function is truly only called when the user triggers it
    }

    public void handleDistancePerBeatChange(float newDistance)
    {
        BPMManager.instance.setDistancePerBeat(newDistance);
    }

    public void handleBPMChangeEnd()
    {
        UsageLogger.log(UserAction.BPM_TEMPO_CHANGED);
    }
    public void handleDistancePerBeatChangeEnd()
    {
        UsageLogger.log(UserAction.BPM_DISTANCE_PER_BEAT_CHANGED);
    }
}
