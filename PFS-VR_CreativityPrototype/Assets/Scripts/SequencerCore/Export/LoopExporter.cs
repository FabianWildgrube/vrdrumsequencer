using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class LoopExporter
{
    static public void exportToJson(string filename, Loop loop)
    {
        var loopExport = new LoopExportData();

        loopExport.participantId = ExperimentManager.instance.currentParticipantId;
        loopExport.conditionName = ExperimentManager.instance.currentConditionAsStr;

        loopExport.loopName = loop.loopName;
        loopExport.exportDate = System.DateTime.Now;

        if (loop.songTrack.song != null)
        {
            loopExport.songName = loop.songTrack.song.name;
            loopExport.songVolume = loop.songTrack.volume;
        }

        loopExport.bpm = BPMManager.instance.bpm;
        loopExport.timeSignatureHi = BPMManager.instance.beatsPerBar;
        loopExport.timeSignatureLo = BPMManager.instance.beatType;
        loopExport.metersPerBeat = BPMManager.instance.distancePerBeat;

        loopExport.durationInBars = loop.durationInBars;

        loopExport.tracks = new List<TrackExportData>();

        foreach (Track t in loop.tracks)
        {
            var trackExport = new TrackExportData();
            trackExport.trackName = t.trackName;
            trackExport.color = new TrackColor() { r = t.color.r, g = t.color.g, b = t.color.b };
            trackExport.sampleDefinition = t.sample.definition;
            trackExport.trackVolume = t.volume;
            trackExport.isMuted = t.isMuted;
            trackExport.isSolo = t.isSolo;
            trackExport.anotherIsSolo = t.anotherIsSolo;

            trackExport.notes = new List<NoteExportData>();

            Note n = t.firstNote;
            while (n != null)
            {
                NoteExportData noteExport = new NoteExportData();
                noteExport.triggerTime = n.zeroBasedTriggerTime();
                noteExport.posOnTrackAxis = (float)(noteExport.triggerTime / (loopExport.durationInBars * BPMManager.instance.secondsPerBar));
                trackExport.notes.Add(noteExport);

                n = n.nextNote;
            }

            loopExport.tracks.Add(trackExport);
        }

        string loopExportJsonString = JsonConvert.SerializeObject(loopExport, Formatting.Indented);
        if (!FileIOUtils.DuplicateSafeWriteToFile(ExperimentManager.instance.loopExportDirPath, filename, "json", loopExportJsonString))
        {
            throw new System.Exception($"Could not export loop to {filename}.json!");
        }
        Debug.Log($"Exported Loop {loop.loopName} to {filename}.json");
    }
}
