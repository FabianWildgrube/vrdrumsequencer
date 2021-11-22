using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SelectionEvent : UnityEvent<BeatSubdivision> { }

public class SubbeatSignatureSelector : DropdownSelector<BeatSubdivision>
{
    [Header("Callback")]
    public SelectionEvent OnSelected;

    void Start()
    {
        OnSelectionChanged += (BeatSubdivision b) => OnSelected.Invoke(b);

        handleSelection(BPMManager.instance != null ? BPMManager.instance.beatSubdivisions : BeatSubdivision.Achtel);
    }
}
