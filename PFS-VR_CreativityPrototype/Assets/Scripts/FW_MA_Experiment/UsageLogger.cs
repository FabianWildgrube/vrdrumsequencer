using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/**
 * Log every action the user can take in the system and save it in a file into the current condition
 */

public enum UserAction
{
    TRACK_ADDED,
    TRACK_DELETED,
    TRACK_VOLUME_CHANGED,
    TRACK_SAMPLE_PREVIEW_PLAYED,
    TRACK_SOLOED,
    TRACK_UNSOLOED,
    TRACK_MUTED,
    TRACK_UNMUTED,
    NOTE_ADDED,
    NOTE_DELETED,
    NOTE_MOVED,
    SAMPLE_MODIFIED,
    SAMPLE_MODIFY_UNDO,
    SAMPLE_MODIFY_REDO,
    SAMPLE_MODIFY_RESHUFFLE,
    SAMPLE_NAME_CHANGED,
    SAMPLE_SAVED_AS_NEW,
    SAMPLE_SAVED_OVERWRITE,
    SAMPLE_SELECTED_NEW,
    SAMPLE_SELECTED_FROM_LIBRARY,
    SAMPLE_EDITOR_MOVED,
    SAMPLE_EDITOR_SCALED,
    LOOP_LOADED_DEFAULT,
    LOOP_LOADED_FROM_DISK,
    LOOP_STARTED,
    LOOP_STOPPED,
    LOOP_EXPORTED,
    LOOP_BAR_ADDED,
    LOOP_CHANGED_NAME,
    LOOP_METRONOME_TURNED_OFF,
    LOOP_METRONOME_TURNED_ON,
    LOOP_SNAP_TO_SUBBEATS_TURNED_ON,
    LOOP_SNAP_TO_SUBBEATS_TURNED_OFF,
    BPM_TEMPO_CHANGED,
    BPM_SIGNATURE_HI_CHANGED,
    BPM_SIGNATURE_LOW_CHANGED,
    BPM_SUBBEAT_SIGNATURE_CHANGED,
    BPM_DISTANCE_PER_BEAT_CHANGED,
    ENVIRONMENT_LOCATION_SELECTED,
    ENVIRONMENT_LIGHTING_TOD_CHANGED,
    ENVIRONMENT_LIGHTING_MAINLIGHT_TEMP_CHANGED,
    ENVIRONMENT_LIGHTING_MAINLIGHT_INTENSITY_CHANGED,
    ENVIRONMENT_LIGHTING_ACCENT_AMOUNT_CHANGED,
    ENVIRONMENT_LIGHTING_ACCENT_COLOR_CHANGED,
    ENVIRONMENT_LIGHTING_ACCENT_INTENSITY_CHANGED,
    ENVIRONMENT_COMPLEXITY_AMOUNT_CHANGED,
    ENVIRONMENT_COMPLEXITY_TIDYNESS_CHANGED,
    ENVIRONMENT_COLORFULLNESS_COLOR_CHANGED,
    ENVIRONMENT_COLORFULLNESS_AMOUNT_CHANGED,
    ENVIRONMENT_AMBIENTSOUND_VOLUME_CHANGED,
    SONG_SELECTED,
    SONG_CHANGED,
    SONG_VOLUME_CHANGED,
    MAIN_MENU_OPENED,
    MAIN_MENU_CLOSED,
    EXPERIMENT_GUIDE_OPENED,
    EXPERIMENT_GUIDE_CLOSED,
    ENVIRONMENT_CONFIG_MENU_OPENED,
    ENVIRONMENT_CONFIG_MENU_CLOSED,
    JUMPED_BACK_TO_INSTRUMENT,
    MOVED_INSTRUMENT_TO_PLAYSPACE
}

class UserActionOccurence
{
    public string action;
    public System.DateTime timestamp;
}

class UsageLog
{
    public string participantId;
    public string condition;
    public System.DateTime startTime;
    public System.DateTime endTime;
    public List<UserActionOccurence> actions = new List<UserActionOccurence>();

    public void add(UserActionOccurence action)
    {
        actions.Add(action);
    }
}

public class UsageLogger : MonoBehaviour
{
    public static UsageLogger instance;

    private UsageLog currentLog;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        currentLog = new UsageLog();
        currentLog.participantId = ExperimentManager.instance.currentParticipantId;
        currentLog.condition = ExperimentManager.instance.currentConditionAsStr;
        currentLog.startTime = System.DateTime.Now;

        ExperimentManager.instance.OnConditionChanged += this.handleConditionChange;
    }

    private void OnDestroy()
    {
        saveCurrentLog();
    }

    public static void log(UserAction action)
    {
        if (instance.currentLog != null)
        {
            var occurence = new UserActionOccurence();
            occurence.action = System.Enum.GetName(typeof(UserAction), action);
            occurence.timestamp = System.DateTime.Now;
            instance.currentLog.add(occurence);
        }
    }

    private void saveCurrentLog()
    {
        if (currentLog != null)
        {
            currentLog.endTime = System.DateTime.Now;
            string exportPath = ExperimentManager.instance.getConditionUserDataDirPath((ExperimentCondition)System.Enum.Parse(typeof(ExperimentCondition), currentLog.condition));
            string usageLogJson = JsonConvert.SerializeObject(currentLog, Formatting.Indented);
            if (!FileIOUtils.DuplicateSafeWriteToFile(exportPath, "usageLog", "json", usageLogJson))
            {
                Debug.LogError("Could not save usageLog to File!");
            }
        }
    }

    void handleConditionChange(ExperimentCondition newCondition)
    {
        saveCurrentLog();

        currentLog = new UsageLog();
        currentLog.participantId = ExperimentManager.instance.currentParticipantId;
        currentLog.condition = System.Enum.GetName(typeof(ExperimentCondition), newCondition);
        currentLog.startTime = System.DateTime.Now;
        
    }
}
