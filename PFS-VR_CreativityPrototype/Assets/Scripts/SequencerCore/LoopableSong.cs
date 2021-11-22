using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "PFS-VR/LoopableSong", order = 3)]
public class LoopableSong : ScriptableObject
{
    public AudioClip clip;
    public string genre;
    public float bpm = 120f;
    public int beatSignatureHi = 4;
    public int beatSignatureLo = 4;
    public int durationInBars = 2;
}