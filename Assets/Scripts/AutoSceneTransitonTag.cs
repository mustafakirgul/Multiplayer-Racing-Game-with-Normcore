using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSceneTransitonTag : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    GameSceneManager gameSceneManager;

    [SerializeField]
    PlayerManager playerManager;
    private void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();

        if (gameSceneManager)
        {
            gameSceneManager.CheckForAutoSceneTransition();
        }

        playerManager = FindObjectOfType<PlayerManager>();

        if (playerManager)
        {
            playerManager.CleanEmptiesInLists();
        }
    }
}
