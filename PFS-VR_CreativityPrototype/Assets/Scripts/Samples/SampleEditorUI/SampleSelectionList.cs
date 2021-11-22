using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
public class SampleSelectionList : MonoBehaviour
{
    private SampleLibrary library;

    public GameObject sampleRepresentationPrototype;
    public ScrollingObjectCollection scrollingCollection;
    public GridObjectCollection sampleRepresentationGrid;
    public SampleDefinitionSelector selectionSignaler;

    private Dictionary<string, GameObject> librarySampleRepresentations = new Dictionary<string, GameObject>();
    private Dictionary<string, SampleSelectRepresentation> sampleSelectors = new Dictionary<string, SampleSelectRepresentation>();

    void Start()
    {
        library = SampleLibraryFactory.getLibrary();

        //init random sample
        GameObject randomRep = Instantiate(sampleRepresentationPrototype, sampleRepresentationGrid.transform);
        SampleSelectRepresentation randomSelector = randomRep.GetComponent<SampleSelectRepresentation>();
        if (randomSelector != null)
        {
            randomSelector.isRandomSample = true;
            randomSelector.OnSelected += this.handleRandomSampleSelected;
            randomSelector.OnPlay += () => this.handleSamplePlayStart(randomSelector);
            sampleSelectors.Add("Random Sample", randomSelector);
        }

        updateSampleList();
        updateUI();

        library.OnSampleDefinitionAdded += this.handleSampleAdded;
        library.OnSampleDefinitionUpdated += this.handleSampleUpdated;
        library.OnSampleDefinitionRemoved += this.handleSampleRemoved;
    }

    private void OnEnable()
    {
        if (library != null)
        {
            updateSampleList();
            updateUI();
        }
    }

    void updateSampleList()
    {
        foreach (SampleDefinition sd in library.SampleDefinitions)
        {
            if (!librarySampleRepresentations.ContainsKey(sd.name))
            {
                GameObject rep = Instantiate(sampleRepresentationPrototype, sampleRepresentationGrid.transform);
                rep.SendMessage("updateData", sd);
                SampleSelectRepresentation selector = rep.GetComponent<SampleSelectRepresentation>();
                if (selector != null)
                {
                    selector.OnSelected += this.handleLibrarySampleSelected;
                    selector.OnPlay += () => this.handleSamplePlayStart(selector);
                    sampleSelectors.Add(sd.name, selector);
                }
                librarySampleRepresentations.Add(sd.name, rep);
            }
        }
    }

    void handleSamplePlayStart(SampleSelectRepresentation triggeringObject)
    {
        foreach (var sampleRep in sampleSelectors.Values)
        {
            if (sampleRep != triggeringObject)
            {
                sampleRep.forceStop();
            }
        }
    }

    void handleLibrarySampleSelected(SampleDefinition sd)
    {
        UsageLogger.log(UserAction.SAMPLE_SELECTED_FROM_LIBRARY);
        selectionSignaler.signalSelection(sd);
    }

    void handleRandomSampleSelected(SampleDefinition sd)
    {
        UsageLogger.log(UserAction.SAMPLE_SELECTED_NEW);
        selectionSignaler.signalSelection(sd);
    }

    void handleSampleAdded(SampleDefinition sd)
    {
        if (gameObject.activeInHierarchy && !librarySampleRepresentations.ContainsKey(sd.name))
        {
            GameObject rep = Instantiate(sampleRepresentationPrototype, sampleRepresentationGrid.transform);
            rep.SendMessage("updateData", sd);
            librarySampleRepresentations.Add(sd.name, rep);
            updateUI();
        }
    }

    void handleSampleUpdated(SampleDefinition sd)
    {
        if (gameObject.activeInHierarchy && librarySampleRepresentations.ContainsKey(sd.name))
        {
            GameObject rep = librarySampleRepresentations[sd.name];
            rep.SendMessage("updateData", sd);
        }
    }

    void handleSampleRemoved(SampleDefinition sd)
    {
        if (librarySampleRepresentations.ContainsKey(sd.name))
        {
            GameObject rep = librarySampleRepresentations[sd.name];
            librarySampleRepresentations.Remove(sd.name);
            SampleSelectRepresentation selector = rep.GetComponent<SampleSelectRepresentation>();
            if (selector != null)
            {
                selector.OnSelected -= this.handleLibrarySampleSelected;
            }
            Destroy(rep);
            updateUI();
        }
    }

    private void updateUI()
    {
        sampleRepresentationGrid.UpdateCollection();
        scrollingCollection.UpdateContent();
    }

    private void OnDestroy()
    {
        library.OnSampleDefinitionAdded -= this.handleSampleAdded;
        library.OnSampleDefinitionUpdated -= this.handleSampleUpdated;
        library.OnSampleDefinitionRemoved -= this.handleSampleRemoved;
    }
}
