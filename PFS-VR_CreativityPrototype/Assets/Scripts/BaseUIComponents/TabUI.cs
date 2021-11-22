using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

[System.Serializable]
public class Tab
{
    public string title;
    public GameObject contentPrefab;
}

public class TabUIData
{
    public GameObject content;
    public Interactable headerBtnInteractable;
    public int idx;
}

public class TabUI : MonoBehaviour
{
    [Header("Tabs")]
    public Tab[] tabDefinitions;
    public Transform tabContentParent;

    [Header("Header Buttons Config")]
    public GameObject tabHeaderBtnPrefab;
    public GridObjectCollection tabHeaderButtonsCollection;

    [Header("Navigation Buttons")]
    public GameObject nextButton;
    public GameObject prevButton;

    public UnityEvent OnAllTabsVisitedAtLeastOnce;
    private bool firedAllTabsVisitedEvent = false;

    private Dictionary<Tab, TabUIData> tabRepresentations;
    private Dictionary<Tab, bool> tabVisitedInfos;

    private int activeIdx = 0;

    private void Awake()
    {
        tabRepresentations = new Dictionary<Tab, TabUIData>();
        tabVisitedInfos = new Dictionary<Tab, bool>();

        Tab firstTab = null;

        int scrollIdx = 0;
        foreach (Tab tab in tabDefinitions)
        {
            if (scrollIdx == 0) firstTab = tab;

            TabUIData tabInfo = new TabUIData();
            tabInfo.idx = scrollIdx;

            GameObject tabContent = Instantiate(tab.contentPrefab, tabContentParent);
            tabContent.SetActive(false);
            tabInfo.content = tabContent;

            GameObject tabHeaderBtn = Instantiate(tabHeaderBtnPrefab, tabHeaderButtonsCollection.transform);

            ButtonConfigHelper headerBtnConfig = tabHeaderBtn.GetComponent<ButtonConfigHelper>();
            if (headerBtnConfig != null)
            {
                headerBtnConfig.MainLabelText = tab.title;
                headerBtnConfig.OnClick.AddListener(() => { handleSelection(tab); });

                tabInfo.headerBtnInteractable = headerBtnConfig.gameObject.GetComponent<Interactable>();
            }

            tabRepresentations.Add(tab, tabInfo);
            tabVisitedInfos.Add(tab, false);

            scrollIdx++;
        }

        tabHeaderButtonsCollection.UpdateCollection();

        handleSelection(firstTab);
    }

    private Tab getByIdx(int idx)
    {
        foreach(var t in tabRepresentations.Keys)
        {
            if (tabRepresentations[t].idx == idx)
            {
                return t;
            }
        }

        return null;
    }

    private void handleSelection(Tab tab)
    {
        if (tab != null)
        {
            activeIdx = tabRepresentations[tab].idx;
            tabVisitedInfos[tab] = true;
            checkIfAllTabsWereVisited();

            prevButton.SetActive(activeIdx != 0);
            nextButton.SetActive(activeIdx < tabRepresentations.Count - 1);

            //update toggle state in header buttons
            foreach (var t in tabRepresentations.Keys)
            {
                tabRepresentations[t].headerBtnInteractable.IsToggled = t == tab;
                tabRepresentations[t].content.SetActive(t == tab);
            }
        }
    }

    private void checkIfAllTabsWereVisited()
    {
        if (!firedAllTabsVisitedEvent)
        {
            bool allVisited = true;
            foreach(var visited in tabVisitedInfos.Values)
            {
                allVisited = allVisited && visited;
            }

            if (allVisited)
            {
                OnAllTabsVisitedAtLeastOnce.Invoke();
                firedAllTabsVisitedEvent = true;
            }
        }
    }

    public void goToNextTab()
    {
        int nextIdx = (activeIdx + 1) % tabRepresentations.Count;
        Tab nextTab = getByIdx(nextIdx);
        if (nextTab != null)
        {
            handleSelection(nextTab);
        }
    }

    public void goToPrevTab()
    {
        int prevIdx = (tabRepresentations.Count + activeIdx - 1) % tabRepresentations.Count;
        Tab prevTab = getByIdx(prevIdx);
        if (prevTab != null)
        {
            handleSelection(prevTab);
        }
    }
}
