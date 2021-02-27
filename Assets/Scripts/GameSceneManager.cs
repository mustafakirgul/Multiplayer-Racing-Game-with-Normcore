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
    public bool m_bisAutomatedSplashes;

    public bool isDebugBuild;
    public int buildFadeTime;
    public int buildSplashTime;

    [SerializeField] private int m_iSplashIndex = 0;

    public Splash[] GameStartSplashes;
    public Splash[] GameEndSplashes;

    bool transitionStarted;
    [SerializeField]
    bool endSequenceActive;

    [SerializeField] private GameObject BlackFadeBox;

    Coroutine endScreenFade;

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
        transform.SetParent(null);
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
        ObtainReferenceToBox();
        //Disable all End screen splashes
        DisableSplashes(GameEndSplashes);
        EnableSplashes(GameStartSplashes);
        //Check if this scene is automatically loaded
        //CheckForEndSequenceTransition();
        BlackFadeBox = transform.GetChild(0).gameObject;

        StartSplashSequence();
    }

    public void OnValidate()
    {
        if (BlackFadeBox == null)
            BlackFadeBox = transform.GetChild(0).gameObject;
        if (isDebugBuild)
        {
            BlackFadeBox.SetActive(false);

            m_fFadeTime = 0;

            for (int i = 0; i < GameStartSplashes.Length; i++)
            {
                GameStartSplashes[i].duration = 0;
            }

            for (int i = 0; i < GameEndSplashes.Length; i++)
            {
                GameEndSplashes[i].duration = 0;
            }
        }
        else
        {
            BlackFadeBox.SetActive(true);

            m_fFadeTime = buildFadeTime;

            for (int i = 0; i < GameStartSplashes.Length; i++)
            {
                GameStartSplashes[i].duration = buildSplashTime;
            }

            for (int i = 0; i < GameEndSplashes.Length; i++)
            {
                GameEndSplashes[i].duration = buildSplashTime;
            }
        }
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
    public void DisableEndSplashes()
    {
        for (int i = 0; i < GameEndSplashes.Length; i++)
        {
            GameEndSplashes[i].splashedGObj.SetActive(false);
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
            StartCoroutine(FadeInAndOutSplash(m_fFadeTime, m_fFadeTime, GameStartSplashes[SplashIndex].duration,
                SplashIndex));
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
    public void StartEndSplashes()
    {
        //Reset splash index
        m_iSplashIndex = 0;
        endSequenceActive = true;
        //Turn off all splashes first
        for (int i = 0; i < GameEndSplashes.Length; i++)
        {
            GameEndSplashes[i].splashedGObj.SetActive(false);
        }

        ShowEndSplash(m_iSplashIndex);
    }
    private void ShowEndSplash(int SplashIndex)
    {
        if (SplashIndex < GameEndSplashes.Length)
        {
            GameEndSplashes[SplashIndex].splashedGObj.SetActive(true);
        }
        else
        {
            endSequenceActive = false;
            Debug.Log("Loading Game Scene");
            //Disable all start splashes
            //Start end sequence here
            LootManager.instance.RollForLoot();
            StartCoroutine(DelaySceneTransiton(0f));
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
        //StartCoroutine(FadeToBlackOutSquare(true, fadeOutTime));
        //    StartCoroutine(DelaySceneTransiton(fadeOutTime, GameStartSplashes));
        //}
    }
    void Update()
    {
        if(endSequenceActive &&
            Input.GetKeyUp(KeyCode.Space))
        {
            CycleEndSequence();
        }
    }
    void CycleEndSequence()
    {
        m_iSplashIndex++;
        ShowEndSplash(m_iSplashIndex);
    }

    public IEnumerator DelaySceneTransiton(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //Instead this should change to the game end canvas or the new screen canvas

        //When the new round loads no longer need intro splashes
       

        //Don't load the scene instead just reconnect and restart the game
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //Select the last build left off before start of the game
        GameManager.instance.uIManager.ReactivateLogin();
        //TO DO: loot drop rolls and assign newly obtainloot to 
        //Display only new weapons that are dropped, avoid duplicating them in the UI
        GameManager.instance.uIManager.AssignAdditionalLootFromGameToDisplay();
        LobbyManager.instance.ConnectToLobby(GameManager.instance.isHost);
        GameManager.instance.uIManager.ClearMessageCache();
    }
}

[Serializable]
public struct Splash
{
    public GameObject splashedGObj;
    public int duration;
}