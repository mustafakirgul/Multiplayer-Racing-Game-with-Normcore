using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingFeedbackText : MonoBehaviour
{
    public List<FadingText> displays;
    public float speed;
    public float distanceFromTarget;
    public bool isTesting;
    private GameObject child;
    [SerializeField] private Transform target, lookAtTarget;
    private StatsEntity se;
    private int counter;
    private bool isAttached, elementsLoaded;
    [Range(0f, 100f)] public float transparencyThreshold;
    private WaitForEndOfFrame waitFrame => new WaitForEndOfFrame();
    private WaitForEndOfFrame wait => new WaitForEndOfFrame();
    private Coroutine cr_MainThread;
    private ComparisonTableColumn previousStats, currentStats;

    void Initialize()
    {
        if (!elementsLoaded) LoadElements();
        if (!isAttached)
        {
            if (cr_MainThread != null) StopCoroutine(cr_MainThread);
            cr_MainThread = StartCoroutine(CR_MainThread());
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!isAttached) return;
        PlaceCanvas();
    }

    private void PlaceCanvas()
    {
        if (target == null) return;
        transform.position =
            target.position + ((lookAtTarget.position - target.position).normalized * distanceFromTarget);
        if (lookAtTarget == null) return;
        transform.LookAt(lookAtTarget);
    }

    private void OnDestroy()
    {
        isAttached = false;
    }

    void LoadElements()
    {
        if (displays == null) displays = new List<FadingText>();
        child = transform.GetChild(0).gameObject;
        if (child != null)
            displays.Add(new FadingText(child.transform.GetChild(0).GetComponent<Text>(),
                child.transform.GetChild(0).GetComponent<CanvasGroup>(),
                child.transform.GetChild(0).GetComponent<RectTransform>(), child, true));
        elementsLoaded = true;
    }

    FadingText CreateNewFadingText()
    {
        if (child != null)
        {
            GameObject tempGO = Instantiate(child, transform);
            FadingText _tempFT = new FadingText(tempGO.transform.GetChild(0).GetComponent<Text>(),
                tempGO.transform.GetChild(0).GetComponent<CanvasGroup>(),
                tempGO.transform.GetChild(0).GetComponent<RectTransform>(), tempGO, true);
            displays.Add(_tempFT);
            return _tempFT;
        }

        return null;
    }

    void AttachToTarget()
    {
        if (PlayerManager.instance == null) return;
        target = PlayerManager.instance.localPlayer;
        if (target == null) return;
        lookAtTarget = Camera.main.transform;
        se = target.transform.GetComponent<Player>().statsEntity;
        currentStats = se.ReturnStats();
        if (se == null) return;
        isAttached = true;
    }

    IEnumerator CR_MainThread()
    {
        while (!isAttached)
        {
            AttachToTarget();
            yield return wait;
        }

        while (isAttached)
        {
            if (target == null)
                isAttached = false;
            if (isTesting)
            {
                ShowFeedbackText("+" + counter + " Cheeseburgers");
                counter++;
            }
            else
            {
                CheckProgress();
            }

            yield return wait;
        }

        yield return null;
    }

    private void CheckProgress()
    {
        previousStats = currentStats;
        currentStats = se.ReturnStats();
        //Debug.LogWarning("Previous Status: k-" + previousStats.kills + " | d-" + previousStats.damage + " | p-" + previousStats.powerup + " | l-" + previousStats.loot);
        //Debug.LogWarning("Current Status: k-" + currentStats.kills + " | d-" + currentStats.damage + " | p-" + currentStats.powerup + " | l-" + currentStats.loot);
        int score =
                (currentStats.kills - previousStats.kills) * 100 +
                (currentStats.damage - previousStats.damage) * 3 +
                (currentStats.loot - previousStats.loot) * 50
            ;
        if (score <= 0) return;
        ShowFeedbackText("+" + score + " points!");
        //Debug.LogWarning("Score registered: " + score);
    }

    private void ShowFeedbackText(string text)
    {
        if (isAttached)
        {
            for (int i = 0; i < displays.Count; i++)
            {
                if (displays[i].isAvailable)
                {
                    StartCoroutine(CR_AnimateText(text, displays[i]));
                    return;
                }
            }

            StartCoroutine(CR_AnimateText(text, CreateNewFadingText()));
            return;
        }

        //Debug.LogWarning("Fading text is not attached to a localPlayer");
    }

    IEnumerator CR_AnimateText(string text, FadingText fText)
    {
        //Debug.LogWarning("CR_AnimateText");
        fText.isAvailable = false;
        fText._gameObject.SetActive(true);
        float Y = 100f;
        fText.cg.alpha = 1f;
        fText.display.text = text;
        while (Y > 0)
        {
            fText.rt.anchoredPosition = new Vector3(0f, -Y, 0f);
            Y -= Time.deltaTime * speed;
            if (Y < transparencyThreshold)
                fText.cg.alpha = Y / transparencyThreshold;
            else
                fText.cg.alpha = 1f;

            yield return waitFrame;
        }

        fText._gameObject.SetActive(false);
        fText.isAvailable = true;
    }
}

[Serializable]
public class FadingText
{
    public FadingText(Text display, CanvasGroup cg, RectTransform rt, GameObject gameObject, bool isAvailable)
    {
        this.display = display;
        this.cg = cg;
        this.rt = rt;
        this.isAvailable = isAvailable;
        _gameObject = gameObject;
    }

    public Text display;
    public CanvasGroup cg;
    public RectTransform rt;
    public bool isAvailable;
    public GameObject _gameObject;
}