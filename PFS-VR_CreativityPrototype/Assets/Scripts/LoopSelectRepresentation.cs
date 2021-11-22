using System;
using System.Collections.Generic;
using UnityEngine;
public struct LoopFileInfo
{
    public string path;
    public string fileName;
    public System.DateTime creationDate;
}
public class LoopSelectRepresentation : MonoBehaviour
{
    private LoopFileInfo info;

    public TMPro.TextMeshPro filenameTxtField;
    public TMPro.TextMeshPro reationDateTxtField;

    public event Action<LoopFileInfo> OnSelected;

    public void init(LoopFileInfo lf)
    {
        info = lf;

        filenameTxtField.text = info.fileName;
        reationDateTxtField.text = info.creationDate.ToString("dd/MM/yyyy HH:mm");
    }

    public void select()
    {
        if (OnSelected != null) OnSelected(info);
    }
}
