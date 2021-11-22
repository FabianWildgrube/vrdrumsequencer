using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System.IO;
using System.Linq;

public class LoopImportSelectionList : MonoBehaviour
{
    public GameObject loopRepresenationPrototype;
    public ScrollingObjectCollection scrollingCollection;
    public GridObjectCollection sampleRepresentationGrid;
    public LoopDataSelector selectionSignaler;

    private List<GameObject> loopSelectorGOs = new List<GameObject>();

    public void showFilesIn(params string[] directories)
    {
        List<FileInfo> loopFiles = new List<FileInfo>();

        foreach (var directory in directories)
        {
            var files = FileIOUtils.getJsonFileInfos(directory);
            loopFiles.AddRange(files);
        }

        int i = 0;
        string[] loopFilePaths = loopFiles.OrderByDescending(f => f.LastWriteTime).Select(f => f.FullName).ToArray();
        foreach(string loopFilePath in loopFilePaths)
        {
            LoopExportData ld = LoopManager.instance.parseLoopDataFromAbsFilepath(loopFilePath);
            LoopFileInfo lf = new LoopFileInfo();
            lf.path = loopFilePath;
            lf.fileName = ld.loopName;
            lf.creationDate = ld.exportDate;

            GameObject selector;

            if (i < loopSelectorGOs.Count)
            {
                selector = loopSelectorGOs[i];
                selector.transform.parent = sampleRepresentationGrid.transform;
                selector.SetActive(true);
            } else
            {
                selector = Instantiate(loopRepresenationPrototype, sampleRepresentationGrid.transform);
                loopSelectorGOs.Add(selector);
            }

            LoopSelectRepresentation selectRep = selector.GetComponent<LoopSelectRepresentation>();
            if (selectRep != null)
            {
                selectRep.init(lf);
                selectRep.OnSelected += this.handleLoopSelected;
            }
            i++;
        }

        while (i < loopSelectorGOs.Count)
        {
            loopSelectorGOs[i].transform.parent = null;
            loopSelectorGOs[i].SetActive(false);
            i++;
        }

        sampleRepresentationGrid.UpdateCollection();
        scrollingCollection.UpdateContent();
    }

    private void OnDestroy()
    {
        foreach (GameObject selector in loopSelectorGOs)
        {
            LoopSelectRepresentation selectRep = selector.GetComponent<LoopSelectRepresentation>();
            if (selectRep != null)
            {
                selectRep.OnSelected -= this.handleLoopSelected;
            }
        }
    }

    private void handleLoopSelected(LoopFileInfo lf)
    {
        selectionSignaler.signalSelection(LoopManager.instance.parseLoopDataFromAbsFilepath(lf.path));
    }
}
