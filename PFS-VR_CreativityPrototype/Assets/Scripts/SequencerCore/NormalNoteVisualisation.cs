using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Note))]
public class NormalNoteVisualisation : MonoBehaviour
{
    public GameObject visualisationObject;
    public float emissiveIntensity = 0.95f;
    Renderer r;

    private float maxScale;

    private void Awake()
    {
        r = visualisationObject.GetComponent<Renderer>();
        if (r == null)
        {
            Debug.LogError("NoteVisualisation Object did not have a renderer!");
            return;
        }
        Note n = GetComponent<Note>();
        n.OnStartedPlaying += () => StartCoroutine(ScaleUpAndDown());
        n.OnInitialized += (Color c) =>
        {
            r.material.EnableKeyword("_EMISSION");
            r.material.color = c;
            r.material.SetColor("_EmissionColor", new Color(c.r * emissiveIntensity, c.g * emissiveIntensity, c.b * emissiveIntensity, 1f));
        };
    }

    private void Start()
    {
        BPMManager.instance.OnDistancePerBeatChange += this.resizeUponBPMParamsChanged;

        maxScale = visualisationObject.transform.localScale.x;
        resizeUponBPMParamsChanged(1f);
    }

    private void OnDestroy()
    {
        BPMManager.instance.OnDistancePerBeatChange -= this.resizeUponBPMParamsChanged;
    }

    private void resizeUponBPMParamsChanged(float ignoreParameter)
    {
        //float parameter only necessary to fullfill event handler interface
        float targetScale = BPMManager.instance.distancePerBeat / 4.0f;
        float minScale = 0.015f;

        float newScale = Mathf.Clamp(targetScale, minScale, maxScale);
        visualisationObject.transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    private IEnumerator ScaleUpAndDown()
    {
        Debug.Log("Scale Note Visualisation!");
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = initialScale * 1.1f;
        float duration = 0.2f;

        for (float time = 0; time < duration * 2; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, duration) / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            yield return null;
        }
        transform.localScale = initialScale;
    }
}
