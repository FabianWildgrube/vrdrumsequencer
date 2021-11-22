using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public struct SongToClip
{
    public string songName;
    public string clipFileName;
}

public struct SongClipMapping
{
    public List<SongToClip> mappings;
}

public class SongLibrary : MonoBehaviour
{
    public static SongLibrary instance;

    [HideInInspector]
    public List<LoopableSong> songs => _songs.ToList().FindAll(s => !songsToExclude.Contains(s));

    private LoopableSong[] _songs;

    private List<LoopableSong> songsToExclude = new List<LoopableSong>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            _songs = Resources.LoadAll<LoopableSong>("Songs");

            SongClipMapping mapping = new SongClipMapping();
            mapping.mappings = new List<SongToClip>();

            foreach (var song in _songs)
            {
                SongToClip stc = new SongToClip();
                stc.songName = song.name;
                stc.clipFileName = song.clip.name;
                mapping.mappings.Add(stc);
            }

            string loopExportJsonString = JsonConvert.SerializeObject(mapping, Formatting.Indented);
            FileIOUtils.DuplicateSafeWriteToFile(Application.persistentDataPath, "SongLibrary", "json", loopExportJsonString);
        }
    }

    public void markSongAsUsed(LoopableSong song)
    {
        if (ExperimentManager.instance.songsSelectedNowShouldNotBeVisibleToThisParticipantAnymore)
        {
            songsToExclude.Add(song);
        }
    }
 
    public LoopableSong getByName(string songname)
    {
        foreach (LoopableSong song in _songs)
        {
            if (song.name == songname) return song;
        }

        return null;
    }
}
