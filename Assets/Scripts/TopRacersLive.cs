using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TopRacersLive : MonoBehaviour
{
    public float interval;
    public TopRacerLine[] displays;
    private Coroutine cr_Check;
    private WaitForSeconds wait;
    public bool isRunning;
    private StatsEntry[] results;
    private List<OrderedEntry> orderedResults;
    [Space(10)] public StatsEntry[] stats;
    StringBuilder sb = new StringBuilder("", 666);
    public StatsManager statsManager;

    public void Status(bool state)
    {
        if (state == isRunning) return;
        ClearDisplays();
        switch (state)
        {
            case true:
                if (!isRunning)
                {
                    StartCheck();
                }

                break;
            case false:
                if (isRunning)
                {
                    StopCheck();
                }

                break;
        }
    }

    private void StopCheck()
    {
        isRunning = false;
        if (cr_Check != null) StopCoroutine(cr_Check);
    }

    public void StartCheck()
    {
        isRunning = true;
        if (interval <= 0)
            interval = 1f;
        wait = new WaitForSeconds(interval);
        if (cr_Check != null) StopCoroutine(cr_Check);
        if (statsManager == null) statsManager = StatsManager.instance;
        if (statsManager == null) statsManager = FindObjectOfType<StatsManager>();

        cr_Check = StartCoroutine(CR_Check());
    }

    private void ClearDisplays()
    {
        if (displays != null)
            for (int i = 0; i < displays.Length; i++)
            {
                HideDisplayLine(i);
            }
    }

    void UpdateDisplay(int index, string _name, int score)
    {
        UnHideDisplayLine(index);
        if (_name == GameManager.instance.playerName)
        {
            displays[index].name.transform.parent.parent.GetChild(0).gameObject.SetActive(true);
            displays[index].name.transform.parent.parent.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            displays[index].name.transform.parent.parent.GetChild(0).gameObject.SetActive(false);
            displays[index].name.transform.parent.parent.GetChild(1).gameObject.SetActive(true);
        }

        displays[index].name.text = _name;
        displays[index].score.text = score.ToString();
    }

    void HideDisplayLine(int index)
    {
        displays[index].name.text = "";
        displays[index].score.text = "";
        displays[index].name.transform.parent.parent.GetChild(0).gameObject.SetActive(false);
        displays[index].name.transform.parent.parent.GetChild(1).gameObject.SetActive(true);
        displays[index].name.transform.parent.parent.gameObject.SetActive(false);
    }

    void UnHideDisplayLine(int index)
    {
        displays[index].name.text = "";
        displays[index].score.text = "";
        displays[index].name.transform.parent.parent.GetChild(0).gameObject.SetActive(false);
        displays[index].name.transform.parent.parent.GetChild(1).gameObject.SetActive(true);
        displays[index].name.transform.parent.parent.gameObject.SetActive(true);
    }

    private IEnumerator CR_Check()
    {
        while (isRunning)
        {
            GetResults();
            yield return wait;
        }
    }

    private void GetResults()
    {
        //Debug.LogWarning("Got Results");
        if (displays == null)
        {
            Debug.LogWarning("No displays to show top racers!");
            return;
        }

        //return ordered results
        if (statsManager == null)
        {
            Debug.LogWarning("Stats Manager instance is missing!");
            return;
        }

        if (orderedResults == null)
            orderedResults = new List<OrderedEntry>();
        else
            orderedResults.Clear();
        orderedResults.AddRange(statsManager.ReturnOrderedStats());
        var resultLength = orderedResults.Count;
        var length = displays.Length;
        //convert information to string to show it on the game screen live!
        for (int i = 0; i < length; i++)
        {
            if (i < resultLength)
            {
                displays[i].name.transform.parent.gameObject.SetActive(true);
                UpdateDisplay(i, orderedResults[i].result.playerName, orderedResults[i].score);
            }
            else
            {
                HideDisplayLine(i);
            }
        }
    }
}

[Serializable]
public struct TopRacerLine
{
    public Text name;
    public Text score;
}