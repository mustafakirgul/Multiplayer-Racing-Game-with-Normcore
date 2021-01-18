using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AutoSceneTransitonTag : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    GameSceneManager gameSceneManager;

    [SerializeField]
    PlayerManager playerManager;
    private void Start()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();

        if (gameSceneManager)
        {
            gameSceneManager.CheckForEndSequenceTransition();
        }

        playerManager = FindObjectOfType<PlayerManager>();

        if (playerManager)
        {
            playerManager.CleanEmptiesInLists();
        }
    }
}
