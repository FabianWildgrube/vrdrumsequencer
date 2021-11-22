using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;

public class SampleEditorSpawnManager : MonoBehaviour
{
    public static SampleEditorSpawnManager instance;

    public GameObject sampleEditorPrototype;
    public Transform editorSpawnArea;
    public GridObjectCollection editorLayoutPositionsGridCollection;
    private Vector3 nextSpawnPosition;

    private Dictionary<string, SampleEditor> sampleEditors = new Dictionary<string, SampleEditor>();
    private Dictionary<string, Transform> layoutTransforms = new Dictionary<string, Transform>();

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

    public SampleEditor getEditor(string name)
    {
        return sampleEditors.ContainsKey(name) ? sampleEditors[name] : null;
    }

    public SampleEditor createNewSampleEditor(Sample sample, Color c)
    {
        GameObject editorObj = Instantiate(sampleEditorPrototype, nextSpawnPosition, Quaternion.identity, editorSpawnArea);
        editorObj.transform.Translate(Vector3.down * 10); //temporarily translate below ground until the "real" position is determined in updateLayout() which needs to happen deferred because of MRTK limitations
        SampleEditor editorComp = editorObj.GetComponent<SampleEditor>();
        if (editorComp != null)
        {
            sampleEditors.Add(sample.name, editorComp);
            editorComp.init(sample, c);

            GameObject layoutPlaceholder = new GameObject(editorObj.name + "_placeholder");
            layoutPlaceholder.transform.parent = editorLayoutPositionsGridCollection.transform;
            layoutTransforms.Add(sample.name, layoutPlaceholder.transform);

            StartCoroutine(updateLayout());

            return editorComp;
        } else
        {
            Debug.LogError("Prefab for SampleEditor did not contain a SampleEditor Component!");
            return null;
        }
    }

    public void deleteSampleEditor(string name)
    {
        if (sampleEditors.ContainsKey(name))
        {
            GameObject editorObj = sampleEditors[name].gameObject;
            GameObject layoutTransObj = layoutTransforms[name].gameObject;
            sampleEditors.Remove(name);
            layoutTransforms.Remove(name);

            Destroy(editorObj);
            Destroy(layoutTransObj);
            StartCoroutine(updateLayout());
        }
    }

    public void clear()
    {
        foreach(SampleEditor editor in sampleEditors.Values)
        {
            Destroy(editor.gameObject);
        }

        foreach (Transform layoutTrans in layoutTransforms.Values)
        {
            Destroy(layoutTrans.gameObject);
        }

        sampleEditors.Clear();
        layoutTransforms.Clear();
        StartCoroutine(updateLayout());
    }

    public IEnumerator updateLayout()
    {
        //defer by two frames to allow MRTK grid component to register newly added editors properly
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        //compute new ideal positions
        if (sampleEditors.Count > 0)
        {
            editorLayoutPositionsGridCollection.Columns = System.Math.Min(sampleEditors.Count, 3);
            if (sampleEditors.Count > 6)
            {
                editorLayoutPositionsGridCollection.Radius = 6f;
            }
            else if (sampleEditors.Count > 3)
            {
                editorLayoutPositionsGridCollection.Radius = 4f;
            } else
            {
                editorLayoutPositionsGridCollection.Radius = 2.5f;
            }
            editorLayoutPositionsGridCollection.UpdateCollection();
        }

        //apply to the editors if they haven't been moved
        foreach(var editorName in sampleEditors.Keys)
        {
            var editor = sampleEditors[editorName];
            if (!editor.transformCustomizedByUser)
            {
                editor.transform.position = layoutTransforms[editorName].position;
                editor.transform.rotation = layoutTransforms[editorName].rotation;
            }
        }
    }

    public void movePlaySpaceToSampleEditor(string sampleName)
    {
        var targetPosition = editorSpawnArea.position;
        Vector3 targetRotation = editorSpawnArea.rotation.eulerAngles; //euler angles to get the y component further down

        if (sampleEditors.ContainsKey(sampleName))
        {
            var editor = sampleEditors[sampleName].GetComponent<SampleEditor>();
            targetPosition = editor.getPlaySpaceTargetPositionForTeleportation();
            targetRotation = editor.getPlaySpaceLookAtRotationForTeleportation();
        }

        float height = targetPosition.y;
        targetPosition -= CameraCache.Main.transform.position - MixedRealityPlayspace.Position;
        targetPosition.y = height;

        MixedRealityPlayspace.Position = targetPosition;
        MixedRealityPlayspace.RotateAround(
                    CameraCache.Main.transform.position,
                    Vector3.up,
                    targetRotation.y - CameraCache.Main.transform.eulerAngles.y);
    }
}
