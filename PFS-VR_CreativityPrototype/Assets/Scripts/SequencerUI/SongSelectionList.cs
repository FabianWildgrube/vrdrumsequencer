using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;

public class SongSelectionList : MonoBehaviour
{
    public GameObject songRepresentationPrototype;
    public ScrollingObjectCollection scrollingCollection;
    public GridObjectCollection songRepresentationGrid;
    public SongSelector selectionSignaler;

    private Dictionary<string, SongSelectRepresentation> songRepresentations = new Dictionary<string, SongSelectRepresentation>();

    void Start()
    {
        //add all songs to list
        foreach (LoopableSong song in SongLibrary.instance.songs)
        {
            GameObject rep = Instantiate(songRepresentationPrototype, songRepresentationGrid.transform);
            SongSelectRepresentation selector = rep.GetComponent<SongSelectRepresentation>();
            if (selector != null)
            {
                selector.init(song);
                selector.OnSelected += this.handleSongSelected;
                selector.OnPlay += () => this.handleSongPlayStart(selector);
                songRepresentations.Add(song.name, selector);
            }
        }

        updateUI();
    }

    private void OnEnable()
    {
        foreach(var songRep in songRepresentations.Values)
        {
            if (!SongLibrary.instance.songs.Contains(songRep.Song))
            {
                Destroy(songRep.gameObject);
            }
        }

        StartCoroutine(deferredUpdateUI());
    }

    private IEnumerator deferredUpdateUI()
    {
        yield return new WaitForSeconds(0.02f);
        updateUI();
    }

    void handleSongPlayStart(SongSelectRepresentation triggeringObject)
    {
        foreach (var songRep in songRepresentations.Values)
        {
            if (songRep != triggeringObject)
            {
                songRep.forceStop();
            }
        }
    }

    void handleSongSelected(LoopableSong song)
    {
        UsageLogger.log(UserAction.SAMPLE_SELECTED_FROM_LIBRARY);
        selectionSignaler.signalSelection(song);
    }

    private void updateUI()
    {
        songRepresentationGrid.UpdateCollection();
        scrollingCollection.UpdateContent();
    }
}

