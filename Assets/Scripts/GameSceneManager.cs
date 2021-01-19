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

    public Splash[] GameStartSplashes;
    public Splash[] EndGameSplashes;

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
        StopAllCoroutines();
        transitionStarted = false;
        //Obtain reference to fade box
        ObtainReferenceToBox();
        //Disable all End screen splashes
        DisableSplashes(EndGameSplashes);
        EnableSplashes(GameStartSplashes);
        //Check if this scene is automatically loaded
        //CheckForEndSequenceTransition();

        StartSplashSequence();
    }

    public void EnableSplashes(Splash[] splashesToEnable)
    {
        for (int i = 0; i < splashesToEnable.Length; i++)
        {
            splashesToEnable[i].splashedGObj.SetActive(true);
        }
    }

    public void DisableSplashes(Splash[] splashesToDisable)
    {
        for (int i = 0; i < splashesToDisable.Length; i++)
        {
            splashesToDisable[i].splashedGObj.SetActive(false);
        }
    }

    private void ObtainReferenceToBox()
    {
        if (blackSquareBox == null)
        {
            blackSquareBox = transform.GetChild(0).gameObject;
        }
    }
    private void StartSplashSequence()
    {
        if (m_bisAutomatedSplashes)
        {
            //Turn off all splashes first
            for (int i = 0; i < GameStartSplashes.Length; i++)
            {
                GameStartSplashes[i].splashedGObj.SetActive(false);
            }

            ShowSplash(m_iSplashIndex);
        }
        else
        {

        }
    }
    private void ShowSplash(int SplashIndex)
    {
        if (SplashIndex < GameStartSplashes.Length)
        {
            StartCoroutine(FadeInAndOutSplash(m_fFadeTime, m_fFadeTime, GameStartSplashes[SplashIndex].duration, SplashIndex));
        }
        else
        {
            Debug.Log("Loading Game Scene");
            //Disable all start splashes
            DisableSplashes(GameStartSplashes);
            //Fade to Game Scene
            StartCoroutine(FadeToBlackOutSquare(false, 2));
        }
    }
    public IEnumerator FadeToBlackOutSquare(bool fadeToBlack, float fadeSpeedTime)
    {
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

    public IEnumerator FadeInAndOut(float fadeIntime, float fadeOutTime, float duration)
    {
        yield return StartCoroutine(FadeToBlackOutSquare(false, fadeIntime));
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
    }

    public IEnumerator FadeInAndOutSplash(float fadeIntime, float fadeOutTime, float duration, int Index)
    {
        GameStartSplashes[Index].splashedGObj.SetActive(true);
        yield return StartCoroutine(FadeToBlackOutSquare(false, fadeIntime));
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));

        //if (m_iSplashIndex + 1 < GameStartSplashes.Length)
        //{
        m_iSplashIndex++;
        ShowSplash(m_iSplashIndex);
        //}
        //else
        //{
        //    //load next scene
        //    StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
        //    StartCoroutine(DelaySceneTransiton(fadeOutTime, GameStartSplashes));
        //}
    }

    public void CheckForEndSequenceTransition()
    {
        if (!transitionStarted)
        {
            //transitionStarted = true;
            if (FindObjectOfType<AutoSceneTransitonTag>())
            {
                //StartCoroutine(DelaySceneTransiton(m_fAutoTransitionDelay, EndGameSplashes));
                //Debug.Log("Auto Scene Transition Tag Found, Changing Scenes in " +
                //    m_fAutoTransitionDelay);
                //Need a new wait to restart and reload the game
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
    public IEnumerator DelaySceneTransiton(float waitTime, Splash[] splashToDisable)
    {
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < splashToDisable.Length; i++)
        {
            splashToDisable[i].splashedGObj.SetActive(false);
        }

        //Instead this should change to the game end canvas or the new screen canvas
        
        //Don't load the scene instead just reconnect and restart the game
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        //When the new round loads no longer need intro splashes
        DisableSplashes(GameStartSplashes);
    }
}

[Serializable]
public struct Splash
{
    public GameObject splashedGObj;
    public int duration;
}
