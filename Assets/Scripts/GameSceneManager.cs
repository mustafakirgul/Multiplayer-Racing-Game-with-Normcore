using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public GameObject blackSquareBox;
    public float m_fAutoTransitionDelay;
    public int m_fFadeTime;

    [SerializeField]
    bool m_bisAutomatedSplashes;

    [SerializeField]
    private int m_iSplashIndex = 0;

    [SerializeField]
    private List<GameObject> m_SplashScreensList = new List<GameObject>();

    public Splash[] splashes;


    #region Singleton Logic
    public static GameSceneManager instance = null;
    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        //Check if this is the only instance of a singleton
        SingletonCheck();

        //Obtain reference to fade box
        blackSquareBox = transform.GetChild(0).gameObject;

        //Check if this scene is automatically loaded
        CheckForEndSequenceTransition();

        StartSplashSequence();
    }

    private void StartSplashSequence()
    {
        if (m_bisAutomatedSplashes)
        {
            //Turn off all splashes first
            for (int i = 0; i < splashes.Length; i++)
            {
                splashes[i].splashedGObj.SetActive(false);
            }

            ShowSplash(m_iSplashIndex);
        }
    }
    private void ShowSplash(int SplashIndex)
    {
        if (SplashIndex < splashes.Length)
        {
            StartCoroutine(FadeInAndOutSplash(m_fFadeTime, m_fFadeTime, splashes[SplashIndex].duration, SplashIndex));
        }
        else
        {
            Debug.Log("Loading Next Scene");
            StopAllCoroutines();
            StartCoroutine(DelaySceneTransiton(1f, SceneManager.GetActiveScene().buildIndex + 1));
            //Array.Clear(splashes, 0, splashes.Length);
        }
    }
    public IEnumerator FadeToBlackOutSquare(bool fadeToBlack, int fadeSpeedTime)
    {
        if (blackSquareBox == null)
        {
            blackSquareBox = transform.GetChild(0).gameObject;
        }

        Color fadeColor = blackSquareBox.GetComponent<Image>().color;
        float fadeAmt;

        if (fadeToBlack)
        {
            //Fade Out (Screen turns black)
            while (blackSquareBox.GetComponent<Image>().color.a < 1)
            {
                fadeAmt = fadeColor.a + (Time.deltaTime / fadeSpeedTime);

                fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmt);
                blackSquareBox.GetComponent<Image>().color = fadeColor;
                yield return null;
            }
        }
        else
        {
            //Fade In (Screen turns into image)
            while (blackSquareBox.GetComponent<Image>().color.a > 0)
            {
                fadeAmt = fadeColor.a - (Time.deltaTime / fadeSpeedTime);

                fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmt);
                blackSquareBox.GetComponent<Image>().color = fadeColor;
                yield return null;
            }
        }
    }

    public IEnumerator FadeInAndOut(int fadeIntime, int fadeOutTime, int duration)
    {
        StartCoroutine(FadeToBlackOutSquare(false, fadeIntime));
        yield return new WaitForSeconds(duration);
        StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
    }

    public IEnumerator FadeInAndOutSplash(int fadeIntime, int fadeOutTime, int duration, int Index)
    {
        splashes[Index].splashedGObj.SetActive(true);
        StartCoroutine(FadeToBlackOutSquare(false, fadeIntime));
        yield return new WaitForSeconds(fadeIntime + duration);
        StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
        yield return new WaitForSeconds(fadeOutTime);
        m_iSplashIndex++;
        ShowSplash(m_iSplashIndex);
    }

    public void CheckForEndSequenceTransition()
    {
        if (FindObjectOfType<AutoSceneTransitonTag>())
        {
            StartCoroutine(DelaySceneTransiton(m_fAutoTransitionDelay,
                (SceneManager.GetActiveScene().buildIndex - 1)));
            Debug.Log("Auto Scene Transition Tag Found, Changing Scenes in " +
                m_fAutoTransitionDelay);
        }
        else
        {
            Debug.Log("No auto Scene Transition Tag Found");
            //Do nothing, no auto transition
            //Wait for manual transition
            //Manual Scene Transition Code here
        }
    }
    public IEnumerator DelaySceneTransiton(float waitTime, int sceneIndex)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneIndex);
    }
}

[Serializable]
public struct Splash
{
    public GameObject splashedGObj;
    public int duration;
}
