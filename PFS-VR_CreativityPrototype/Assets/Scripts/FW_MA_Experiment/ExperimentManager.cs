using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Microsoft.MixedReality.Toolkit.UI;

/**
 * This file containts the implementation of a liner flow management through the Experiment for Fabian Wildgrubes Master Thesis.
 * The ExperimentManager class provides services to the rest of the application:
 *      - create and provide directories for persistent user data, that are automatically unique to the current participant
 *      - activate and deactive stages of the experiment in the game's scene
 * 
 * The Experiment has distinct stages, which are run through by a single participant in a linear fashion. A participant can not turn back.
 * The order of the stages in each condition is encoded in the class ExperimentOrder.
 * 
 * OverallExperimentState (i.e. how many participants have alread done the experiment, when and with which starting condition)
 * is persisted in a json file. The class OverallExperimentState is the model for that. This class also provides logic to:
 *      - make sure each participant gets a unique pseudonym
 *      - ensure a random and equally distributed starting order (which condition comes first for which participant) 
 */

public enum ExperimentStage
{
    INTRO_VR,
    INTRO_PFS_CONDUCTOR,
    INTRO_PFS_USER,
    OVERVIEW_OF_STEPS,
    ENVIRONMENT_CONFIGURATION,
    EXPLORATION_01,
    PERFECTION_01,
    EXPLORATION_02,
    PERFECTION_02,
    QUESTIONNAIRE_01,
    QUESTIONNAIRE_02,
    DONE
}

public enum ExperimentCondition
{
    PRE,
    FIXED_ENVIRONMENT,
    CUSTOM_ENVIRONMENT,
    POST
}

public class ExperimentOrder
{
    static List<Tuple<ExperimentCondition, ExperimentStage>> fixedConditionFirstStages =  new List<Tuple<ExperimentCondition, ExperimentStage>> {
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.INTRO_VR),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.INTRO_PFS_CONDUCTOR),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.INTRO_PFS_USER),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.OVERVIEW_OF_STEPS),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.FIXED_ENVIRONMENT, ExperimentStage.EXPLORATION_01),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.FIXED_ENVIRONMENT, ExperimentStage.PERFECTION_01),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.FIXED_ENVIRONMENT, ExperimentStage.QUESTIONNAIRE_01),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.ENVIRONMENT_CONFIGURATION),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.EXPLORATION_02),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.PERFECTION_02),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.QUESTIONNAIRE_02),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.POST, ExperimentStage.DONE)
    };

    static List<Tuple<ExperimentCondition, ExperimentStage>> customConditionFirstStages = new List<Tuple<ExperimentCondition, ExperimentStage>> {
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.INTRO_VR),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.INTRO_PFS_CONDUCTOR),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.INTRO_PFS_USER),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.PRE, ExperimentStage.OVERVIEW_OF_STEPS),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.ENVIRONMENT_CONFIGURATION),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.EXPLORATION_01),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.PERFECTION_01),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.CUSTOM_ENVIRONMENT, ExperimentStage.QUESTIONNAIRE_01),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.FIXED_ENVIRONMENT, ExperimentStage.EXPLORATION_02),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.FIXED_ENVIRONMENT, ExperimentStage.PERFECTION_02),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.FIXED_ENVIRONMENT, ExperimentStage.QUESTIONNAIRE_02),
        new Tuple<ExperimentCondition, ExperimentStage>(ExperimentCondition.POST, ExperimentStage.DONE)
    };

    static public List<Tuple<ExperimentCondition, ExperimentStage>> getOrder(ExperimentCondition firstCondition)
    {
        return firstCondition == ExperimentCondition.CUSTOM_ENVIRONMENT ? customConditionFirstStages: fixedConditionFirstStages;
    }
}


[System.Serializable]
public class ParticipantInfo
{
    public string pseudonym;
    public System.DateTime startTime;
    public System.DateTime endTime;
    public ExperimentCondition startedWithCondition;
    public string startedWithConditionName;
}

[System.Serializable]
public class OverallExperimentState
{
    public List<ParticipantInfo> participants;

    /// <summary>
    /// Determine start condition: Either override value from experiment manager or automatic deduction.
    /// Automatic:
    ///     if other participants exist, take the newest and start with the opposite
    ///     if this is the first participant start with a random condition
    /// </summary>
    /// <returns></returns>
    private ExperimentCondition nextParticipantStartCondition()
    {
        if (ExperimentManager.instance.startCondition != StartCondition.AUTO)
        {
            return ExperimentManager.instance.startCondition == StartCondition.FIXED_ENV ? ExperimentCondition.FIXED_ENVIRONMENT : ExperimentCondition.CUSTOM_ENVIRONMENT;
        } else if (participants != null && participants.Count > 0)
        {
            return participants[participants.Count - 1].startedWithCondition == ExperimentCondition.CUSTOM_ENVIRONMENT ? ExperimentCondition.FIXED_ENVIRONMENT : ExperimentCondition.CUSTOM_ENVIRONMENT;
        } else
        {
            return UnityEngine.Random.value > 0.5f ? ExperimentCondition.FIXED_ENVIRONMENT : ExperimentCondition.CUSTOM_ENVIRONMENT;
        }
    }

    private string createUniquePseudonym()
    {
        string pseudonym = ExperimentManager.instance.useManualParticipantName ? ExperimentManager.instance.manualParticipantName : Utils.NewRandomName;
        int ctr = 1;
        string initialPseudonym = pseudonym;
        while (participants.Find(p => p.pseudonym == pseudonym) != null)
        {
            Debug.LogWarning("Participant with the same pseudonym already exists (" + pseudonym + ")!");
            ExperimentManager.instance.showWarning("Participant with the same pseudonym already exists (" + pseudonym + ")!");
            pseudonym = initialPseudonym + "_" + ctr;
            ++ctr;
        }
        return pseudonym;
    }

    public ParticipantInfo createNewParticipant()
    {
        if (participants == null) participants = new List<ParticipantInfo>();

        ParticipantInfo p = new ParticipantInfo();
        p.startTime = System.DateTime.Now;
        p.startedWithCondition = nextParticipantStartCondition();
        p.startedWithConditionName = Enum.GetName(typeof(ExperimentCondition), p.startedWithCondition);
        p.pseudonym = createUniquePseudonym();

        participants.Add(p);

        return p;
    }
}

//Helper class to enable "Dictionary" like entry of a mapping between stages and game objects in the editor
[Serializable]
public class ExperimentStageMapping
{
    public ExperimentStage stage;
    public GameObject[] associatedObjects;
}

public enum StartCondition
{
    AUTO,
    FIXED_ENV,
    CUSTOM_ENV,
}

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    public bool useManualParticipantName = false;
    public string manualParticipantName = "";

    public GameObject warningToast;
    public TMPro.TextMeshPro warningToastText;

    public bool ignoreStageInitDelayComponents = false;
    public bool ignoreStageLeaveChecks = false;
    [Tooltip("Which environment condition should come first. Select Auto to automatically alternate with each new Game start.")]
    public StartCondition startCondition =  StartCondition.AUTO;

    public ExperimentCondition[] dontExcludeSongsSelectedInTheseConditions;

    #region Private FilePaths
    private string _overallExperimentStateFilePath => Path.Combine(GlobalConstants.appDataDirPath, "overallExperimentState.json");
    private string _generalUserDataDirPath => Path.Combine(GlobalConstants.appDataDirPath, GlobalConstants.participantDataFolderName, currentParticipant.pseudonym);
    private string _fixedConditionUserDataDirPath => Path.Combine(GlobalConstants.appDataDirPath, GlobalConstants.participantDataFolderName, currentParticipant.pseudonym, "Condition_Fixed");
    private string _customConditionUserDataDirPath => Path.Combine(GlobalConstants.appDataDirPath, GlobalConstants.participantDataFolderName, currentParticipant.pseudonym, "Condition_Custom");
    #endregion

    #region Public API
    public string generalUserDataDirPath => _generalUserDataDirPath;
    public string currentConditionUserDataDirPath { get
        {
            if (currentCondition == ExperimentCondition.CUSTOM_ENVIRONMENT )
            {
                return _customConditionUserDataDirPath;
            } else if (currentCondition == ExperimentCondition.FIXED_ENVIRONMENT)
            {
                return _fixedConditionUserDataDirPath;
            } else
            {
                return _generalUserDataDirPath;
            }
        }
    }

    public string getConditionUserDataDirPath(ExperimentCondition condition)
    {
        if (condition == ExperimentCondition.CUSTOM_ENVIRONMENT)
        {
            return _customConditionUserDataDirPath;
        }
        else if (condition == ExperimentCondition.FIXED_ENVIRONMENT)
        {
            return _fixedConditionUserDataDirPath;
        }
        else
        {
            return _generalUserDataDirPath;
        }
    }

    public string loopExportDirPath { get
        {
            string baseDir = currentConditionUserDataDirPath;
            string loopDir = "Loops";
            
            if (currentExperimentStage == ExperimentStage.EXPLORATION_01 || currentExperimentStage == ExperimentStage.EXPLORATION_02)
            {
                loopDir = GlobalConstants.explorationLoopsExportFolderName;
            } else if (currentExperimentStage == ExperimentStage.PERFECTION_01 || currentExperimentStage == ExperimentStage.PERFECTION_02)
            {
                loopDir = GlobalConstants.perfectionLoopsExportFolderName;
            }

            return Path.Combine(baseDir, loopDir);
        } }

    public string[] currentUserAndParticipantLoopDirs { get
        {
            var paths = new string[] {  Path.Combine(currentConditionUserDataDirPath, GlobalConstants.explorationLoopsExportFolderName),
                                        Path.Combine(currentConditionUserDataDirPath, GlobalConstants.perfectionLoopsExportFolderName) };
            return paths;
        } }
    public ExperimentStage currentExperimentStage => stageOrderForParticipant.Count > 0 ? stageOrderForParticipant[currentStageIdx].Item2 : ExperimentStage.DONE;
    public ExperimentCondition currentCondition => stageOrderForParticipant[currentStageIdx].Item1;
    public string currentConditionAsStr => Enum.GetName(typeof(ExperimentCondition), currentCondition);
    public string currentParticipantId => currentParticipant != null ? currentParticipant.pseudonym : "";
    public Action<ExperimentCondition> OnConditionChanged;
    public Action<ExperimentStage> OnStageChanged;

    public bool perfectEnvironmentShouldBeUsed { get
        {
            return
                currentCondition == ExperimentCondition.FIXED_ENVIRONMENT
                &&
                (currentExperimentStage == ExperimentStage.EXPLORATION_01
                || currentExperimentStage == ExperimentStage.EXPLORATION_02
                || currentExperimentStage == ExperimentStage.PERFECTION_01
                || currentExperimentStage == ExperimentStage.PERFECTION_02);
        } }

    public bool defaultEnvironmentShouldBeUsed { get
        {
            return
                currentCondition == ExperimentCondition.PRE
                || currentCondition == ExperimentCondition.POST
                || currentExperimentStage == ExperimentStage.QUESTIONNAIRE_01
                || currentExperimentStage == ExperimentStage.QUESTIONNAIRE_02;
        } }

    public bool songsSelectedNowShouldNotBeVisibleToThisParticipantAnymore
    {
        get
        {
            return !dontExcludeSongsSelectedInTheseConditions.Contains(currentCondition);
        }
    }
    #endregion

    [SerializeField]
    private ExperimentStage startStage = ExperimentStage.INTRO_PFS_CONDUCTOR;

    [SerializeField]
    private ProgressIndicatorLoadingBar progressIndicator;

    [SerializeField]
    ExperimentStageMapping[] stagesMappingIn = Enum.GetValues(typeof(ExperimentStage)).Cast<ExperimentStage>().Select(stage => { var mapping = new ExperimentStageMapping(); mapping.stage = stage; return mapping; }).ToArray();

    private Dictionary<ExperimentStage, List<GameObject>> _stagesMapping;

    private Dictionary<ExperimentStage, Func<bool>> _stagesLeavePredicates;

    private OverallExperimentState overallExperimentState;
    private ParticipantInfo currentParticipant = null;
    private List<Tuple<ExperimentCondition, ExperimentStage>> stageOrderForParticipant;
    private int currentStageIdx = 0;

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
            _stagesMapping = new Dictionary<ExperimentStage, List<GameObject>>();
            _stagesLeavePredicates = new Dictionary<ExperimentStage, Func<bool>>();
        }

        //Create Dictionary from Unity Editor input list
        foreach (var stageData in stagesMappingIn)
        {
            try
            {
                _stagesMapping.Add(stageData.stage, new List<GameObject>(stageData.associatedObjects));
                _stagesMapping[stageData.stage].ForEach(g => g.SetActive(false));
            } catch(Exception)
            {
                Debug.LogError("Can't have multiple stages of the same type in experiment: " + stageData.stage);
            }
        }

        //setup predicates
        _stagesLeavePredicates.Add(ExperimentStage.EXPLORATION_01, isAllowedToLeaveExploration01);
        _stagesLeavePredicates.Add(ExperimentStage.EXPLORATION_02, isAllowedToLeaveExploration02);
        _stagesLeavePredicates.Add(ExperimentStage.PERFECTION_01, isAllowedToLeavePerfection01);
        _stagesLeavePredicates.Add(ExperimentStage.PERFECTION_02, isAllowedToLeavePerfection02);

        loadOverallExperimentState();

        //Create necessary stuff for the next participant
        currentParticipant = overallExperimentState.createNewParticipant();
        Directory.CreateDirectory(_generalUserDataDirPath);
        Directory.CreateDirectory(_fixedConditionUserDataDirPath);
        Directory.CreateDirectory(_customConditionUserDataDirPath);
        Directory.CreateDirectory(Path.Combine(_generalUserDataDirPath, "Loops"));
        Directory.CreateDirectory(Path.Combine(_fixedConditionUserDataDirPath, GlobalConstants.explorationLoopsExportFolderName));
        Directory.CreateDirectory(Path.Combine(_fixedConditionUserDataDirPath, GlobalConstants.perfectionLoopsExportFolderName));
        Directory.CreateDirectory(Path.Combine(_customConditionUserDataDirPath, GlobalConstants.explorationLoopsExportFolderName));
        Directory.CreateDirectory(Path.Combine(_customConditionUserDataDirPath, GlobalConstants.perfectionLoopsExportFolderName));

        stageOrderForParticipant = ExperimentOrder.getOrder(currentParticipant.startedWithCondition);

        if (stageOrderForParticipant.Count == 0)
        {
            Debug.LogError("There must be at least one stage in the experiment");
            return;
        }

        if (_stagesMapping.Count > 0)
        {
            currentStageIdx = stageOrderForParticipant.FindIndex(t => t.Item2 == startStage);
        }
    }

    private void Start()
    {
        if (_stagesMapping.Count > 0)
        {
            progressIndicator.Progress = (float)(currentStageIdx + 1) / (float)_stagesMapping.Count;
            progressIndicator.OpenAsync();
            _stagesMapping[currentExperimentStage].ForEach(this.showStageObject);
        }

        if (ignoreStageLeaveChecks)
        {
            showWarning("Stage Leaves Checks sind DEAKTIVIERT!");
        }

        if (ignoreStageInitDelayComponents)
        {
            showWarning("Stage Init Delays werden ignoriert!");
        }
    }

    public void OnDestroy()
    {
        currentParticipant.endTime = System.DateTime.Now;
        string overallExperimentStateJson = JsonConvert.SerializeObject(overallExperimentState, Formatting.Indented);
        //explicit write to overwrite the existing file with the new version
        if (!FileIOUtils.WriteToFile(_overallExperimentStateFilePath, overallExperimentStateJson))
        {
            Debug.LogError("Could not save overallExperimentState to File!");
        }
    }

    private void loadOverallExperimentState()
    {
        overallExperimentState = FileIOUtils.createFromJsonFile<OverallExperimentState>(_overallExperimentStateFilePath);
    }

    public void proceedToNextStage()
    {
        if (_stagesMapping.Count == 0 || currentExperimentStage == ExperimentStage.DONE) return;

        if (!ignoreStageLeaveChecks)
        {
            //check if we're allowed to leave the current stage
            if (_stagesLeavePredicates.ContainsKey(currentExperimentStage) && !_stagesLeavePredicates[currentExperimentStage]())
            {
                showWarningThatStageIsNotCompleted();
                return;
            }
        }

        //hide old stage
        _stagesMapping[currentExperimentStage].ForEach(this.hideStageObject);

        var oldCondition = currentCondition;

        ++currentStageIdx;
        progressIndicator.Progress = (float)(currentStageIdx + 1) / (float)_stagesMapping.Count;

        //make sure a next stage exists
        if (currentStageIdx < stageOrderForParticipant.Count)
        {
            var nextStage = stageOrderForParticipant[currentStageIdx].Item2;
            if (_stagesMapping.ContainsKey(nextStage))
            {
                _stagesMapping[currentExperimentStage].ForEach(this.showStageObject);
                if (currentCondition != oldCondition)
                {
                    if (OnConditionChanged != null) OnConditionChanged(currentCondition);
                }
            }
            if (OnStageChanged != null) OnStageChanged(currentExperimentStage);
        }
    }

    private void showStageObject(GameObject stageObject)
    {
        stageObject.SetActive(true);
        var initializers = stageObject.GetComponents<IStageInitializer>();
        foreach (var initializer in initializers)
        {
            initializer.InitExperimentStage(currentExperimentStage);
        }
    }

    private void hideStageObject(GameObject stageObject)
    {
        stageObject.SetActive(false);
    }

    public bool suppressWarnings = false;

    public void showWarning(string message)
    {
        if (!suppressWarnings)
        {
            if (warningToast.activeSelf)
            {
                //append message to existing one
                warningToastText.text += " && \n" + message;
            }
            else
            {
                warningToast.SetActive(true);
                warningToastText.text = message;
            }
        }
    }

    #region checks for allowing proceeding through the experiment

    private void showWarningThatStageIsNotCompleted()
    {
        switch (currentExperimentStage)
        {
            case ExperimentStage.EXPLORATION_01:
                showWarning("Du müsstest in der aktuellen Phase bitte mindestens 3 verschiedene Loops erstellen und speichern, bevor Du weitermachen kannst. Du hast bisher " + loopsSavedDuringExploration01 + " Loops gespeichert.");
                break;
            case ExperimentStage.PERFECTION_01:
                showWarning("Du müsstest " + (loopsLoadedDuringPerfection01 == 0 ? "bitte einen Deiner Loops aus der freien Exploration laden, bearbeiten und dann " : "Deinen Loop noch") +  " speichern, bevor Du weitermachen kannst.");
                break;
            case ExperimentStage.EXPLORATION_02:
                showWarning("Du müsstest in der aktuellen Phase bitte mindestens 3 verschiedene Loops erstellen und speichern, bevor Du weitermachen kannst. Du hast bisher " + loopsSavedDuringExploration02 + " Loops gespeichert.");
                break;
            case ExperimentStage.PERFECTION_02:
                showWarning("Du müsstest " + (loopsLoadedDuringPerfection02 == 0 ? "bitte einen Deiner Loops aus der freien Exploration laden, bearbeiten und dann " : "Deinen Loop bitte noch") + " speichern, bevor Du weitermachen kannst.");
                break;
            default:
                //in other stages we don't have any problems
                break;
        }
    }

    int loopsSavedDuringExploration01 = 0;
    int newLoopsBegunDuringExploration01 = 0;
    int loopsSavedDuringExploration02 = 0;
    int newLoopsBegunDuringExploration02 = 0;
    int loopsSavedDuringPerfection01 = 0;
    int loopsLoadedDuringPerfection01 = 0;
    int loopsSavedDuringPerfection02 = 0;
    int loopsLoadedDuringPerfection02 = 0;

    public void registerLoopWasSaved()
    {
        Debug.Log("registerLoopWasSaved() in " + System.Enum.GetName(typeof(ExperimentStage), currentExperimentStage));
        switch (currentExperimentStage)
        {
            case ExperimentStage.EXPLORATION_01:
                ++loopsSavedDuringExploration01;
                Debug.Log("loopsSavedDuringExploration01: " + loopsSavedDuringExploration01);
                break;
            case ExperimentStage.PERFECTION_01:
                ++loopsSavedDuringPerfection01;
                Debug.Log("loopsSavedDuringPerfection01: " + loopsSavedDuringPerfection01);
                break;
            case ExperimentStage.EXPLORATION_02:
                ++loopsSavedDuringExploration02;
                Debug.Log("loopsSavedDuringExploration02: " + loopsSavedDuringExploration02);
                break;
            case ExperimentStage.PERFECTION_02:
                ++loopsSavedDuringPerfection02;
                Debug.Log("loopsSavedDuringPerfection02: " + loopsSavedDuringPerfection02);
                break;
            default:
                //ignore
                break;
        }
    }

    public void registerNewDefaultLoopLoaded()
    {
        Debug.Log("registerNewDefaultLoopLoaded() in " + System.Enum.GetName(typeof(ExperimentStage), currentExperimentStage));
        switch (currentExperimentStage)
        {
            case ExperimentStage.EXPLORATION_01:
                ++newLoopsBegunDuringExploration01;
                Debug.Log("newLoopsBegunDuringExploration01: " + newLoopsBegunDuringExploration01);
                break;
            case ExperimentStage.EXPLORATION_02:
                ++newLoopsBegunDuringExploration02;
                Debug.Log("newLoopsBegunDuringExploration01: " + newLoopsBegunDuringExploration01);
                break;
            default:
                //ignore
                break;
        }
    }

    public void registerCustomLoopLoaded()
    {
        Debug.Log("registerCustomLoopLoaded() in " + System.Enum.GetName(typeof(ExperimentStage), currentExperimentStage));
        switch (currentExperimentStage)
        {
            case ExperimentStage.PERFECTION_01:
                ++loopsLoadedDuringPerfection01;
                Debug.Log("loopsLoadedDuringPerfection01: " + loopsLoadedDuringPerfection01);
                break;
            case ExperimentStage.PERFECTION_02:
                ++loopsLoadedDuringPerfection02;
                Debug.Log("loopsLoadedDuringPerfection02: " + loopsLoadedDuringPerfection02);
                break;
            default:
                //ignore
                break;
        }
    }

    private bool isAllowedToLeaveExploration01()
    {
        //must have saved at least three different loops during stage exploration 01
        Debug.Log("isAllowedToLeaveExploration01(): loopsSavedDuringExploration01: " + loopsSavedDuringExploration01 + ", newLoopsBegunDuringExploration01:" + newLoopsBegunDuringExploration01);
        return loopsSavedDuringExploration01 >= 3 && newLoopsBegunDuringExploration01 >= 2;
    }

    private bool isAllowedToLeaveExploration02()
    {
        //must have saved at least three different loops during stage exploration 02
        Debug.Log("isAllowedToLeaveExploration02(): loopsSavedDuringExploration02: " + loopsSavedDuringExploration02 + ", newLoopsBegunDuringExploration02:" + newLoopsBegunDuringExploration02);
        return loopsSavedDuringExploration02 >= 3 && newLoopsBegunDuringExploration02 >= 2;
    }

    private bool isAllowedToLeavePerfection01()
    {
        //must have opened and saved at least one loop during perfection 01
        Debug.Log("isAllowedToLeavePerfection01(): loopsLoadedDuringPerfection01: " + loopsLoadedDuringPerfection01 + ", loopsSavedDuringPerfection01:" + loopsSavedDuringPerfection01);
        return loopsLoadedDuringPerfection01 >= 1 && loopsSavedDuringPerfection01 >= 1;
    }

    private bool isAllowedToLeavePerfection02()
    {
        //must have opened and saved at least one loop during perfection 02
        Debug.Log("isAllowedToLeavePerfection02(): loopsLoadedDuringPerfection02: " + loopsLoadedDuringPerfection02 + ", loopsSavedDuringPerfection02:" + loopsSavedDuringPerfection02);
        return loopsLoadedDuringPerfection02 >= 1 && loopsSavedDuringPerfection02 >= 1;
    }

    #endregion
}
