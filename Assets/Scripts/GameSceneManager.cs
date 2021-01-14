using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public GameObject blackSquareBox;

    public float m_fAutoTransitionDelay;

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
        SingletonCheck();
        blackSquareBox = transform.GetChild(0).gameObject;
        CheckForAutoSceneTransition();
    }

    public IEnumerator FadeToBlackOutSquare(bool fadeToBlack, int fadeSpeed)
    {
        Color fadeColor = blackSquareBox.GetComponent<Image>().color;
        float fadeAmt;

        if (fadeToBlack)
        {
            //Fade In
            while (blackSquareBox.GetComponent<Image>().color.a < 1)
            {
                fadeAmt = fadeColor.a + (fadeSpeed * Time.deltaTime);

                fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmt);
                blackSquareBox.GetComponent<Image>().color = fadeColor;
                yield return null;
            }
        }
        else
        {
            //Fade Out
            while (blackSquareBox.GetComponent<Image>().color.a > 0)
            {
                fadeAmt = fadeColor.a - (fadeSpeed * Time.deltaTime);

                fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmt);
                blackSquareBox.GetComponent<Image>().color = fadeColor;
                yield return null;
            }
        }
    }

    public IEnumerator FadeInAndOut(int fadeSpeed1, int fadeSpeed2)
    {
        StartCoroutine(FadeToBlackOutSquare(true, fadeSpeed1));
        yield return new WaitForSeconds(2f);
        StartCoroutine(FadeToBlackOutSquare(false, fadeSpeed2));
    }
    public void ChangeScenes(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void CheckForAutoSceneTransition()
    {
        if (FindObjectOfType<AutoSceneTransitonTag>())
        {
            StartCoroutine(DelaySceneTransiton(m_fAutoTransitionDelay,
                (SceneManager.GetActiveScene().buildIndex - 1)
                ));
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
        ChangeScenes(sceneIndex);
    }
}
