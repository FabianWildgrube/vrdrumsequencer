using System.Collections.Generic;
using UnityEngine;

public class LoopGrid : MonoBehaviour
{
    public GameObject beatStartLinePrototype;
    public GameObject mainBeatLinePrototype;
    public GameObject subBeatLinePrototype;

    public float baseHeight = 0.0f;

    [HideInInspector]
    public float trackHeight; //has to be set up by another script before first use!

    private List<BeatGridLine> beatStartLinesPool = new List<BeatGridLine>();
    private List<BeatGridLine> mainBeatLinesPool = new List<BeatGridLine>();
    private List<BeatGridLine> subBeatLinesPool = new List<BeatGridLine>();

    public void relayoutLines(int nrOfBars, int nrOfTracks)
    {
        //check if we have enough
        int nrOfStartLines = nrOfBars;
        int beatSubdivisionInt = (int)BPMManager.instance.beatSubdivisions;
        int nrOfMainBeatLines = nrOfBars * (BPMManager.instance.beatsPerBar - 1);
        int nrOfSubBeatLines = nrOfBars * BPMManager.instance.beatsPerBar * (beatSubdivisionInt - 1);

        while(beatStartLinesPool.Count < nrOfStartLines)
        {
            var line = Instantiate(beatStartLinePrototype, transform);
            beatStartLinesPool.Add(line.GetComponent<BeatGridLine>());
        }

        while(mainBeatLinesPool.Count < nrOfMainBeatLines)
        {
            var line = Instantiate(mainBeatLinePrototype, transform);
            mainBeatLinesPool.Add(line.GetComponent<BeatGridLine>());
        }

        while(subBeatLinesPool.Count < nrOfSubBeatLines)
        {
            var line = Instantiate(subBeatLinePrototype, transform);
            subBeatLinesPool.Add(line.GetComponent<BeatGridLine>());
        }

        //hide all lines -> later only activate the ones actually needed
        foreach (BeatGridLine line in beatStartLinesPool)
        {
            line.gameObject.SetActive(false);
        }

        foreach (BeatGridLine line in mainBeatLinesPool)
        {
            line.gameObject.SetActive(false);
        }

        foreach (BeatGridLine line in subBeatLinesPool)
        {
            line.gameObject.SetActive(false);
        }

        float newHeight = baseHeight + (nrOfTracks * trackHeight);

        Vector3 insertPos = transform.position;
        Vector3 step = transform.forward * BPMManager.instance.distancePerBar;
        {
            int barNr = 0;
            int barStartLineIdx = 0;
            while (barNr < nrOfBars)
            {
                beatStartLinesPool[barStartLineIdx].gameObject.SetActive(true);
                beatStartLinesPool[barStartLineIdx].transform.position = insertPos;
                beatStartLinesPool[barStartLineIdx].setHeight(newHeight);
                beatStartLinesPool[barStartLineIdx].setLabel($"{barNr + 1}");
                barStartLineIdx++;

                insertPos += step;
                barNr++;
            }
        }

        {
            insertPos = transform.position;
            step = transform.forward * BPMManager.instance.distancePerBeat;
            int beatNr = 0;
            int beatLineIdx = 0;
            while (beatNr < nrOfBars * BPMManager.instance.beatsPerBar)
            {
                if (beatNr % BPMManager.instance.beatsPerBar != 0) //beat lines not on first beat of a bar
                {
                    mainBeatLinesPool[beatLineIdx].gameObject.SetActive(true);
                    mainBeatLinesPool[beatLineIdx].transform.position = insertPos;
                    mainBeatLinesPool[beatLineIdx].setHeight(newHeight);
                    mainBeatLinesPool[beatLineIdx].setLabel($"{(beatNr % BPMManager.instance.beatsPerBar) + 1}");
                    beatLineIdx++;
                }

                insertPos += step;
                beatNr++;
            }
        }

        if (beatSubdivisionInt > 1)
        {
            insertPos = transform.position;
            step = transform.forward * BPMManager.instance.distancePerSubBeat;
            int subBeatNr = 0;
            int subBeatLineIdx = 0;
            while (subBeatNr < nrOfBars * BPMManager.instance.beatsPerBar * beatSubdivisionInt)
            {
                if (subBeatNr % beatSubdivisionInt != 0) //sub beat lines only between beats
                {
                    subBeatLinesPool[subBeatLineIdx].gameObject.SetActive(true);
                    subBeatLinesPool[subBeatLineIdx].transform.position = insertPos;
                    subBeatLinesPool[subBeatLineIdx].setHeight(newHeight);
                    subBeatLineIdx++;
                }

                insertPos += step;
                subBeatNr++;
            }
        }
    }
}
