using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshPro))]
public class TracknameTxtField : MonoBehaviour
{
    public Track track;
    private TMPro.TextMeshPro textElement;

    // Start is called before the first frame update
    void Start()
    {
        textElement = GetComponent<TMPro.TextMeshPro>();
        textElement.text = track.trackName;
    }
}
