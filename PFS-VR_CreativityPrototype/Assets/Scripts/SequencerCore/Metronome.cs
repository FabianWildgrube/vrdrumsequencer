using UnityEngine;

// The code example shows how to implement a metronome that procedurally
// generates the click sounds via the OnAudioFilterRead callback.
// While the game is paused or suspended, this time will not be updated and sounds
// playing will be paused. Therefore developers of music scheduling routines do not have
// to do any rescheduling after the app is unpaused

[RequireComponent(typeof(AudioSource))]
public class Metronome : MonoBehaviour
{
    public double bpm = 140.0F;
    public float gain = 0.5F;
    public int signatureHi = 4;
    public int signatureLo = 4;
    public bool muted = false;

    private double nextTick = 0.0F;
    private float amp = 0.0F;
    private float phase = 0.0F;
    private double sampleRate = 0.0F;
    private int accent;
    private bool running = false;

    private double samplesPerTick => sampleRate * 60.0F / bpm * 4.0F / signatureLo;

    private void Start()
    {
        recalculateTimingInfo(); //initialize
        BPMManager.instance.OnBPMChange += this.handleBPMChange;
        BPMManager.instance.OnBeatsPerBarChange += this.handleBeatSignatureChange;
        BPMManager.instance.OnBeatTypeChange += this.handleBeatSignatureChange;
    }

    private void OnDestroy()
    {
        BPMManager.instance.OnBPMChange -= this.handleBPMChange;
        BPMManager.instance.OnBeatsPerBarChange -= this.handleBeatSignatureChange;
        BPMManager.instance.OnBeatTypeChange -= this.handleBeatSignatureChange;
    }

    public void StartTicking()
    {
        accent = signatureHi;
        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;
        nextTick = startTick * sampleRate;
        running = true;
    }

    public void StopTicking()
    {
        running = false;
    }

    private void handleBPMChange(float ignore)
    {
        recalculateTimingInfo();
    }

    private void handleBeatSignatureChange(int ignore)
    {
        recalculateTimingInfo();
    }

    private void recalculateTimingInfo()
    {
        if (running)
        {
            //get progress between two ticks of the old tempo
            double prevTick = nextTick - samplesPerTick;
            double currentSample = AudioSettings.dspTime * sampleRate;
            double progressWithinTick = (currentSample - prevTick) / samplesPerTick;

            //apply the new tempo
            bpm = BPMManager.instance.bpm;
            signatureHi = BPMManager.instance.beatsPerBar;
            signatureLo = BPMManager.instance.beatType;

            //calculate the next tick
            nextTick = currentSample + (1.0 - progressWithinTick) * samplesPerTick;
        } else
        {
            //simply apply the new tempo, everythin else will be worked out when running starts again
            bpm = BPMManager.instance.bpm;
            signatureHi = BPMManager.instance.beatsPerBar;
            signatureLo = BPMManager.instance.beatType;
        }        
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!running)
            return;

        double sample = AudioSettings.dspTime * sampleRate;
        int dataLen = data.Length / channels;

        int n = 0;
        while (n < dataLen)
        {
            float x = gain * amp * Mathf.Sin(phase);
            int i = 0;
            while (i < channels)
            {
                if (!muted) data[n * channels + i] += x;
                i++;
            }
            while (sample + n >= nextTick)
            {
                nextTick += samplesPerTick;
                amp = 1.0F;
                if (++accent > signatureHi)
                {
                    accent = 1;
                    amp *= 2.0F;
                }
            }
            phase += amp * 0.3F;
            amp *= 0.993F;
            n++;
        }
    }
}