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

    public Splash[] splashes;

    bool transitionStarted;
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
        transitionStarted = false;
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
            StartCoroutine(DelaySceneTransiton(5f, SceneManager.GetActiveScene().buildIndex + 1));
            Array.Clear(splashes, 0, splashes.Length);
            splashes = new Splash[0];
        }
    }
    public IEnumerator FadeToBlackOutSquare(bool fadeToBlack, int fadeSpeedTime)
    {
        //if (blackSquareBox == null)
        //{
        //    blackSquareBox = transform.GetChild(0).gameObject;
        //}

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
        yield return StartCoroutine(FadeToBlackOutSquare(false, fadeIntime));
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
    }

    public IEnumerator FadeInAndOutSplash(int fadeIntime, int fadeOutTime, int duration, int Index)
    {
        splashes[Index].splashedGObj.SetActive(true);
        yield return StartCoroutine(FadeToBlackOutSquare(false, fadeIntime));
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
        if (m_iSplashIndex + 1 < splashes.Length)
        {
            m_iSplashIndex++;
            ShowSplash(m_iSplashIndex);
        }
        else
        {
            //load next scene
            StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
            StartCoroutine(DelaySceneTransiton(fadeOutTime, 1));
        }
    }

    public void CheckForEndSequenceTransition()
    {
        if (!transitionStarted)
        {
            transitionStarted = true;
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
    }
    public IEnumerator DelaySceneTransiton(float waitTime, int sceneIndex)
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(FadeToBlackOutSquare(false, 1));
        SceneManager.LoadScene(sceneIndex);
    }
}

[Serializable]
public struct Splash
{
    public GameObject splashedGObj;
    public int duration;
}
