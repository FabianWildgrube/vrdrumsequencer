using System;
using UnityEngine;

public class LoopDataSelector : MonoBehaviour
{
    public event Action<LoopExportData> OnLoopDataSelected;

    public LoopImportSelectionList list;

    public void signalSelection(LoopExportData ld)
    {
        if (OnLoopDataSelected != null) OnLoopDataSelected(ld);
    }

    public void loadListFrom(params string[] directories)
    {
        list.showFilesIn(directories);
    }
}
