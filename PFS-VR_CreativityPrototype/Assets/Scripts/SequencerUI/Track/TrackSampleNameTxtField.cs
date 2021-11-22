using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshPro))]
public class TrackSampleNameTxtField : MonoBehaviour
{
    public Track track;
    private TMPro.TextMeshPro textElement;

    // Start is called before the first frame update
    void Start()
    {
        textElement = GetComponent<TMPro.TextMeshPro>();
        textElement.text = track.sample.name;
        track.sample.OnNameChanged += this.updateName;
    }

    private void OnDestroy()
    {
        track.sample.OnNameChanged -= this.updateName;
    }

    void updateName(string newName)
    {
        textElement.text = newName;
    }
}
