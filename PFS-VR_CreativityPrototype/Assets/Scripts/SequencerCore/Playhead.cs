using System.Collections;
using UnityEngine;

public class Playhead : MonoBehaviour
{
    public Transform initialPosTransform;
    public GameObject line;
    public float baseHeight = 0.0f;

    [HideInInspector]
    public float trackHeight; //has to be set up by another script before first use!

    private float _loopLength;

    private float velocity = 0;
    private Vector3 initialPosition { get { return initialPosTransform.position; } }
    private float travelledDistance = 0;
    private bool moving = false;

    private int _durationInBars;

    private void Start()
    {
        BPMManager.instance.OnBPMChange += this.handleBPMChange;
        BPMManager.instance.OnBeatsPerBarChange += this.handleBeatSignatureChange;
        BPMManager.instance.OnBeatTypeChange += this.handleBeatSignatureChange;

        BPMManager.instance.OnDistancePerBeatChange += this.handleBPMChange;
    }

    private void OnDestroy()
    {
        BPMManager.instance.OnBPMChange -= this.handleBPMChange;
        BPMManager.instance.OnBeatsPerBarChange -= this.handleBeatSignatureChange;
        BPMManager.instance.OnBeatTypeChange -= this.handleBeatSignatureChange;

        BPMManager.instance.OnDistancePerBeatChange -= this.handleBPMChange;
    }

    void Update()
    {
        if (moving)
        {
            float distThisFrame = velocity * Time.deltaTime;
            travelledDistance += distThisFrame;
            if (travelledDistance >= _loopLength) //loop around at the end
            {
                float overshoot = travelledDistance - _loopLength;
                distThisFrame = overshoot;
                travelledDistance = distThisFrame;
                transform.position = initialPosition;
            }
            transform.position += transform.forward * distThisFrame;
        }
    }

    public void startMoving()
    {
        moving = true;
    }

    public void stopMoving()
    {
        StopAllCoroutines();
        moving = false;
        travelledDistance = 0;
        transform.position = initialPosition;
    }

    private void recalculateVelocity()
    {
        velocity = (BPMManager.instance.distancePerBeat * BPMManager.instance.bpm) / (60.0f * (4.0f / (float)BPMManager.instance.beatType));
    }

    private void handleBPMChange(float ignore)
    {
        //current state with "old" length
        float currentProgress = (_loopLength > 0f) ? travelledDistance / _loopLength : 0f;

        //update length
        _loopLength = BPMManager.instance.barLength * _durationInBars;
        recalculateVelocity();

        //restore current state to something valid with the new length
        transform.position = initialPosition + transform.forward * currentProgress * _loopLength;
        travelledDistance = currentProgress * _loopLength;
    }

    private void handleBeatSignatureChange(int ignore)
    {
        //no position recalculation necessary
        _loopLength = BPMManager.instance.barLength * _durationInBars;
        recalculateVelocity();
    }

    public void relayout(int nrOfBars, int nrOfTracks)
    {
        _durationInBars = nrOfBars;

        //update length
        _loopLength = BPMManager.instance.barLength * nrOfBars;
        recalculateVelocity();

        //handle height changes
        float newHeight = baseHeight + (nrOfTracks * trackHeight);
        Vector3 oldScale = line.transform.localScale;
        line.transform.localScale = new Vector3(oldScale.x, newHeight, oldScale.z);
    }
}
