using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSceneTransitonTag : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    GameSceneManager gameSceneManager;
    private void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();

        if (gameSceneManager)
        {
            gameSceneManager.CheckForAutoSceneTransition();
        }
    }
}
