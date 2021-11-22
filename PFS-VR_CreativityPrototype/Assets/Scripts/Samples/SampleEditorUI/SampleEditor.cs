using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SampleEditor : MonoBehaviour
{
    #region Value Changers
    public GameObject sampleValueChangerPrototype;
    private List<GameObject> valueChangers = new List<GameObject>();
    [SerializeField()]
    private int nrOfDisplayedChangers = GlobalConstants.NR_OF_VECTOR_VALUES;
    #endregion

    #region Fibonnaci Sphere Parameters
    public Transform fibbonaciSphereCenter;
    public Transform parentForValueChangers;
    public GameObject manipulationBoxCollider;
    public GameObject minLimitSphere;
    public GameObject maxLimitSphere;
    public float fibSphereMinLimitRadius => minLimitSphere.transform.lossyScale.x * 0.5f;
    public float fibSphereMaxLimitRadius => maxLimitSphere.transform.lossyScale.x * 0.5f;
    #endregion

    #region Custom Placement
    public bool transformCustomizedByUser = false;
    #endregion

    #region MenuGUI elements
    public Transform menuContainer;
    public TMPro.TextMeshPro sampleNameTxtField;
    public GameObject SaveNewBtn;
    public GameObject SaveAsCopyBtn;
    public GameObject SaveOverwriteBtn;
    #endregion

    public Transform teleportToMeTarget;

    public Sample sample;
    private AudioSource audioSrc;
    private Color color;

    private Vector3 fibSphereInitialPos;
    private Vector3 fibSphereInitialLocalScale;
    private Quaternion fibSphereInitialRotation;

    private Vector3 menuInitialPos;
    private Vector3 menuInitialLocalScale;
    private Quaternion menuInitialRotation;

    private bool firstTimeWeReceiveClipData = true;

    /// Setup the editor. Expected to be called immediately after Instantiation.
    public void init(Sample sample, Color c)
    {
        color = c;
        audioSrc = GetComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        sampleNameTxtField.text = sample.name;
        this.sample = sample;
        this.sample.OnClipChanged += (AudioClip newClip) =>
        {
            this.audioSrc.clip = newClip;

            //because we get the clip over the network, the first time we get the data already triggers this callback
            //but we don't want to play the sound then because all active editors trigger the play at roughly the same time, leading to audio chaos
            if (!firstTimeWeReceiveClipData) 
            {
                this.Play();
            } else
            {
                firstTimeWeReceiveClipData = false;
            }
        };

        //give the max sphere the color of the sample
        var maxSphereRenderer = maxLimitSphere.GetComponent<Renderer>();
        var currentAlpha = maxSphereRenderer.material.color.a;
        maxSphereRenderer.material.color = new Color(c.r, c.g, c.b, currentAlpha);

        if (SampleLibraryFactory.getLibrary().contains(sample.definition))
        {
            updateBtnsForExistingSample();
        } else
        {
            updateBtnsForNewSample();
        }
    }
    public void handleDisplayedChangersNrChange(float newNrOfChangers)
    {
        int nrOfChangers = (int)Mathf.Clamp(Mathf.Floor(newNrOfChangers), 1f, GlobalConstants.NR_OF_VECTOR_VALUES * 1.0f);
        nrOfDisplayedChangers = nrOfChangers;
        recomputeVisibleFibSphere();
    }

    public void handleUserChangedEditorTransform()
    {
        transformCustomizedByUser = true;
    }

    public void logUserMovedEditor()
    {
        UsageLogger.log(UserAction.SAMPLE_EDITOR_MOVED);
    }

    public void logUserScaledEditor()
    {
        UsageLogger.log(UserAction.SAMPLE_EDITOR_SCALED);
    }

    public void handleUserChangedFibSphereScale()
    {
        //forward that information to the currently active sample value changers
        for (int i = 0; i < nrOfDisplayedChangers; i++)
        {
            int importanceSortedIdx = GlobalConstants.VisualVectorRelevanceIndices[i];
            GameObject changerObj = valueChangers[importanceSortedIdx];
            SampleValueChanger changerComp = changerObj.GetComponent<SampleValueChanger>();
            changerComp.updateScale(parentForValueChangers.localScale.x);
        }
    }

    public void handleResetPosition()
    {
        transformCustomizedByUser = false; //must be before updatLayout!

        //reset overall position (which might have been changed by dragging around)
        SampleEditorSpawnManager.instance.updateLayout();

        parentForValueChangers.transform.position = fibSphereInitialPos;
        parentForValueChangers.transform.localScale = fibSphereInitialLocalScale;
        parentForValueChangers.transform.rotation = fibSphereInitialRotation;

        menuContainer.position = menuInitialPos;
        menuContainer.localScale = menuInitialLocalScale;
        menuContainer.rotation = menuInitialRotation;
    }

    public void ReShuffle()
    {
        sample.update(Utils.getRandomSampleVectorValues());
        UsageLogger.log(UserAction.SAMPLE_MODIFY_RESHUFFLE);
    }

    public Vector3 getPlaySpaceTargetPositionForTeleportation()
    {
        //Perform a raycast to find the actual ground
        RaycastHit hit;
        if (Physics.Raycast(teleportToMeTarget.position, Vector3.down, out hit, Mathf.Infinity, GlobalConstants.groundLayerMask)) //check for ground below
        {
            return hit.point;
        } else if(Physics.Raycast(teleportToMeTarget.position, Vector3.up, out hit, Mathf.Infinity, GlobalConstants.groundLayerMask)) //check above
        {
            return hit.point;
        }
        else
        {
            Debug.LogWarning("Couldn't find ground near sample editor. Teleporting into empty space...");

            //project target pos onto 0 height as a fallback
            Vector3 targetPosOnFloor = Vector3.zero;
            targetPosOnFloor.x = teleportToMeTarget.position.x;
            targetPosOnFloor.z = teleportToMeTarget.position.z;

            return targetPosOnFloor;
        }
    }

    public Vector3 getPlaySpaceLookAtRotationForTeleportation()
    {
        return teleportToMeTarget.rotation.eulerAngles;
    }

    #region Undo/Redo
    public void Undo()
    {
        sample.undoLastUpdate();
        UsageLogger.log(UserAction.SAMPLE_MODIFY_UNDO);
    }

    public void Redo()
    {
        sample.redoLastUndoneUpdate();
        UsageLogger.log(UserAction.SAMPLE_MODIFY_REDO);
    }
    #endregion

    #region Saving sample to library
    public void addToLibrary()
    {
        SampleLibraryFactory.getLibrary().add(sample.definition);
        SampleLibraryFactory.getLibrary().SaveToFile();
        UsageLogger.log(UserAction.SAMPLE_SAVED_AS_NEW);

        updateBtnsForExistingSample();
    }

    public void updateInLibrary()
    {
        SampleLibraryFactory.getLibrary().update(sample.definition);
        SampleLibraryFactory.getLibrary().SaveToFile();
        UsageLogger.log(UserAction.SAMPLE_SAVED_OVERWRITE);
    }
    #endregion

    #region Fibbonaci Sphere Layouting
    void Start()
    {
        initFibSphere();

        fibSphereInitialPos = parentForValueChangers.transform.position;
        fibSphereInitialLocalScale = parentForValueChangers.transform.localScale;
        fibSphereInitialRotation = parentForValueChangers.transform.rotation;

        menuInitialPos = menuContainer.position;
        menuInitialLocalScale = menuContainer.localScale;
        menuInitialRotation = menuContainer.rotation;
    }

    /// instantiates # SampleDefinition.NR_OF_VALUES ValueChangers and positions them along fibSphere
    void initFibSphere()
    {
        manipulationBoxCollider.transform.localScale = Vector3.one * fibSphereMaxLimitRadius * 2.0f;

        List<Vector3> initialFibPositions = Utils.getPositionsOnUnitFibbonaciSphere(GlobalConstants.NR_OF_VECTOR_VALUES).Select(p => { return fibbonaciSphereCenter.position + p; }).ToList();

        int i = 0;
        foreach (Vector3 initialPos in initialFibPositions)
        {
            GameObject newManipulationSphere = Instantiate(sampleValueChangerPrototype, initialPos, Quaternion.identity, parentForValueChangers);
            SampleValueChanger changer = newManipulationSphere.GetComponent<SampleValueChanger>();
            if (changer != null)
            {
                changer.init(i, sample, fibbonaciSphereCenter, initialPos, minLimitSphere.transform, maxLimitSphere.transform, color);
            }
            valueChangers.Add(newManipulationSphere);
            i++;
        }
    }

    /// Shows and repositions nrOfDisplayedChangers valueChangers to form fibSphere; deactivates all remaining changers
    /// Does so using the importance order of the values for the sample
    void recomputeVisibleFibSphere()
    {
        List<Vector3> newPositions = Utils.getPositionsOnUnitFibbonaciSphere(nrOfDisplayedChangers).Select(p => { return fibbonaciSphereCenter.position + p; }).ToList();
        //reposition importance sorted
        for (int i = 0; i < nrOfDisplayedChangers; i++)
        {
            int importanceSortedIdx = GlobalConstants.VisualVectorRelevanceIndices[i];
            GameObject changerObj = valueChangers[importanceSortedIdx];
            changerObj.SetActive(true);
            SampleValueChanger changerComp = changerObj.GetComponent<SampleValueChanger>();
            changerComp.updateToNewBasePosition(newPositions[i]);
            changerComp.updateScale(parentForValueChangers.localScale.x);
        }

        //deactivate all other changers
        for (int j = nrOfDisplayedChangers; j < valueChangers.Count; j++)
        {
            int importanceSortedIdx = GlobalConstants.VisualVectorRelevanceIndices[j];
            GameObject changerObj = valueChangers[importanceSortedIdx];
            changerObj.SetActive(false);
        }
    }
    #endregion

    #region Save as copy dialog flow
    public void SaveAsCopyInLibrary()
    {
        //get a new name
        KeyboardManager.instance.keyboard.show(sample.name + "_copy", "Name for Sample Copy");
        KeyboardManager.instance.keyboard.OnTextCommitted += this.handleSaveAsCopyNameEntry;
        KeyboardManager.instance.keyboard.OnKeyboardHide += this.handleSaveAsCopyCancelled;
    }

    private void handleSaveAsCopyNameEntry(string name)
    {
        //set the name on the definition
        //add that definition to the library
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleSaveAsCopyNameEntry;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleSaveAsCopyCancelled;

        sample.setName(name);
        sampleNameTxtField.text = name;
        addToLibrary();
    }

    private void handleSaveAsCopyCancelled()
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleSaveAsCopyNameEntry;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleSaveAsCopyCancelled;
    }
    #endregion

    #region Name edit Dialog flow
    public void startNameEditFlow()
    {
        KeyboardManager.instance.keyboard.show(sample.name, "Edit Sample name");
        KeyboardManager.instance.keyboard.OnTextCommitted += this.handleNameEdited;
        KeyboardManager.instance.keyboard.OnKeyboardHide += this.handleNameEditFlowCanceled;
    }

    private void handleNameEdited(string newName)
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleNameEdited;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleNameEditFlowCanceled;

        sample.setName(newName);
        sampleNameTxtField.text = newName;
        UsageLogger.log(UserAction.SAMPLE_NAME_CHANGED);

        if (SampleLibraryFactory.getLibrary().contains(newName))
        {
            updateBtnsForExistingSample();
        } else
        {
            updateBtnsForNewSample();
        }
    }

    private void handleNameEditFlowCanceled()
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleNameEdited;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleNameEditFlowCanceled;
    }
    #endregion

    #region helpers
    private void updateBtnsForNewSample()
    {
        SaveAsCopyBtn.SetActive(false);
        SaveOverwriteBtn.SetActive(false);
        SaveNewBtn.SetActive(true);
    }

    private void updateBtnsForExistingSample()
    {
        SaveAsCopyBtn.SetActive(true);
        SaveOverwriteBtn.SetActive(true);
        SaveNewBtn.SetActive(false);
    }

    public void Play()
    {
        audioSrc.Play();
    }

    public void Stop()
    {
        audioSrc.Stop();
    }
    #endregion
}
