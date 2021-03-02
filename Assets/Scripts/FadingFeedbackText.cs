using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingFeedbackText : MonoBehaviour
{
    public List<FadingText> displays;
    public float speed;
    private GameObject child;
    private Transform target, lookAtTarget;
    private StatsEntity se;
    private int counter;
    [Range(0f, 100f)] public float transparencyThreshold;
    private WaitForEndOfFrame waitFrame => new WaitForEndOfFrame();

    void LoadElements()
    {
        if (displays == null) displays = new List<FadingText>();
        child = transform.GetChild(0).gameObject;
        if (child != null)
            displays.Add(new FadingText(child.transform.GetChild(0).GetComponent<Text>(),
                child.transform.GetChild(0).GetComponent<CanvasGroup>(),
                child.transform.GetChild(0).GetComponent<RectTransform>(), child));
        if (PlayerManager.instance == null) return;
        target = PlayerManager.instance.localPlayer;
        if (target == null) return;
        lookAtTarget = Camera.main.transform;
        se = target.transform.GetComponent<Player>().statsEntity;
    }

    FadingText CreateNewFadingText()
    {
        FadingText _temp = new FadingText();
        if (child != null)
        {
            GameObject temp = Instantiate(child, transform);
            _temp = new FadingText(temp.transform.GetChild(0).GetComponent<Text>(),
                temp.transform.GetChild(0).GetComponent<CanvasGroup>(),
                temp.transform.GetChild(0).GetComponent<RectTransform>(), temp);
            displays.Add(_temp);
        }

        return _temp;
    }

    private void Start()
    {
        LoadElements();
        ShowFeedbackText("+" + counter + " Feedbacking");
    }

    private void ShowFeedbackText(string text)
    {
        for (int i = 0; i < displays.Count; i++)
        {
            FadingText temp = displays[i];
            if (temp.isAvailable)
            {
                temp.isAvailable = false;
                StartCoroutine(CR_AnimateText(text, temp));
                return;
            }
        }

        StartCoroutine(CR_AnimateText(text, CreateNewFadingText()));
    }

    IEnumerator CR_AnimateText(string text, FadingText fText)
    {
        fText._gameObject.SetActive(true);
        fText.isAvailable = false;
        float Y = 100f;
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

        fText.isAvailable = true;
        fText._gameObject.SetActive(false);
        counter++;
        ShowFeedbackText("+" + counter + " Feedbacking");
    }
}

[Serializable]
public struct FadingText
{
    public FadingText(Text display, CanvasGroup cg, RectTransform rt, GameObject gameObject)
    {
        this.display = display;
        this.cg = cg;
        this.rt = rt;
        isAvailable = true;
        _gameObject = gameObject;
    }

    public Text display;
    public CanvasGroup cg;
    public RectTransform rt;
    public bool isAvailable;
    public GameObject _gameObject;
}