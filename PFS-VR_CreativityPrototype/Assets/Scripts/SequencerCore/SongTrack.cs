using UnityEngine;

public class SongTrack : MonoBehaviour
{
    [HideInInspector]
    public LoopableSong song;
    public AudioSource audioSource;
    public float visualisationLengthOffset;
    public GameObject visualisation;
    public SongPlayhead playhead;
    public Renderer visualisationRenderer;
    public Color waveformWaveColor;
    public Color waveformBGColor;
    public ValueSliderCtrl volumeSlider;

    public TMPro.TextMeshPro songNameTxtField;
    public GameObject changeSongBtn;
    public GameObject selectSongDialog;
    public SongSelector songSelector;

    private float _length = 0f;
    public float length
    {
        get { return _length; }
        set
        {
            _length = Mathf.Max(0.3f, value + visualisationLengthOffset);
            visualisation.transform.localScale = new Vector3(_length, visualisation.transform.localScale.y, visualisation.transform.localScale.z);
            playhead.length = _length;
        }
    }

    public float volume => audioSource.volume;

    public void handleVolumeChange(float newVolume)
    {
        audioSource.volume = newVolume;
    }

    public void handleVolumeChangeInteractionEnd()
    {
        UsageLogger.log(UserAction.SONG_VOLUME_CHANGED);
    }

    public void showSelectSongDialog()
    {
        selectSongDialog.SetActive(true);
        songSelector.OnSongSelected += this.handleSongDialogSelected;
    }

    private void handleSongDialogSelected(LoopableSong ls)
    {
        if (song == null) UsageLogger.log(UserAction.SONG_SELECTED);
        else UsageLogger.log(UserAction.SONG_CHANGED);
        setSong(ls);
        closeSelectSongDialog();
    }

    public void closeSelectSongDialog()
    {
        songSelector.OnSongSelected -= this.handleSongDialogSelected;
        selectSongDialog.SetActive(false);
    }

    void Start()
    {
        if (song != null)
        {
            updateElements();
        } else
        {
            visualisation.SetActive(false);
            playhead.gameObject.SetActive(false);
        }

        if (ExperimentManager.instance.currentExperimentStage == ExperimentStage.PERFECTION_01 || ExperimentManager.instance.currentExperimentStage == ExperimentStage.PERFECTION_02)
        {
            changeSongBtn.SetActive(false);
        }
        else
        {
            changeSongBtn.SetActive(true);
        }
    }

    public void setSong(LoopableSong newSong)
    {
        song = newSong;
        BPMManager.instance.setBPM(song.bpm, false);
        BPMManager.instance.setBeatsPerBar(song.beatSignatureHi, false);
        BPMManager.instance.setBeatType(song.beatSignatureLo, false);
        updateElements();
    }

    private void updateElements()
    {
        audioSource.clip = song.clip;
        visualisation.SetActive(true);
        visualisationRenderer.material.SetTexture("_MainTex", PaintWaveformSpectrum(song.clip, 1024, 400, waveformWaveColor, waveformBGColor));
        songNameTxtField.text = song.name;
        playhead.gameObject.SetActive(true);
        playhead.clip = song.clip;
        playhead.length = length;
    }

    public void Play()
    {
        audioSource.Play();
        playhead.StartMoving();
    }

    public void Stop()
    {
        audioSource.Stop();
        playhead.StopMoving();
    }

    // Taken from: https://answers.unity.com/questions/699595/how-to-generate-waveform-from-audioclip.html
    private Texture2D PaintWaveformSpectrum(AudioClip audio, int width, int height, Color waveColor, Color bgColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float[] samples = new float[audio.samples];
        float[] waveform = new float[width];
        audio.GetData(samples, 0);
        int packSize = (audio.samples / width) + 1;
        int s = 0;
        for (int i = 0; i < audio.samples; i += packSize)
        {
            waveform[s] = Mathf.Abs(samples[i]) * 4;
            s++;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, bgColor);
            }
        }

        for (int x = 0; x < waveform.Length; x++)
        {
            for (int y = 0; y <= waveform[x] * ((float)height * .95f); y++)
            {
                tex.SetPixel(x, (height / 2) + y, waveColor);
                tex.SetPixel(x, (height / 2) - y, waveColor);
            }
        }
        tex.Apply();

        return tex;
    }
}
