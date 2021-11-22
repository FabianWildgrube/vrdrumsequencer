using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayStopBtn : MonoBehaviour
{
    [SerializeField]
    GameObject playBtn;

    [SerializeField]
    GameObject stopBtn;

    private bool showPlay = true;

    public UnityEvent OnPlay;
    public UnityEvent OnStop;
   

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void onPress()
    {
        showPlay = !showPlay;
        updateUI();
        notifyListeners();
    }

    private void notifyListeners()
    {
        if (showPlay)
        {
            OnStop.Invoke();
        } else
        {
            OnPlay.Invoke();
        }
    }

    private void updateUI()
    {
        playBtn.SetActive(showPlay);
        stopBtn.SetActive(!showPlay);
    }

    public void forceShowPlayState()
    {
        showPlay = true;
        updateUI();
    }
}
