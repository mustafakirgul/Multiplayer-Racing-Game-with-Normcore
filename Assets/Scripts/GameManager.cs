using Normal.Realtime;
using TMPro;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Realtime _realtime;
    public Vector3 minimum, maximum;
    Vector3 spawnPoint;
    public TextMeshProUGUI playerNameInputField;
    public Canvas _enterNameCanvas;
    public Camera _miniMapCamera;
    public string preferredCar;
    string _tempName;
    ChaseCam chaseCam;

    //Game Manager Params
    public float m_fMaxTimer;
    public float m_localTimer;
    public int m_iNumOfPlayersForGameStart;
    [SerializeField]
    public Race _race;
    private PlayerManager playerManager;
    private UIManager uIManager;

    private GameSceneManager gameSceneManager;
    [SerializeField]
    private bool readyToStart;

    [SerializeField]
    private Truck lootTruck;

    //bool isConnected;

    private void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();
        chaseCam = GameObject.FindObjectOfType<ChaseCam>();
        playerManager = FindObjectOfType<PlayerManager>();
        uIManager = FindObjectOfType<UIManager>();
        _enterNameCanvas.gameObject.SetActive(true);
        // Get the Realtime component on this game object
        _realtime = GetComponent<Realtime>();

        // Notify us when Realtime successfully connects to the room
        _realtime.didConnectToRoom += DidConnectToRoom;
        _realtime.didDisconnectFromRoom += DidDisconnectFromRoom;
        spawnPoint = new Vector3(
            UnityEngine.Random.Range(minimum.x, maximum.x),
            UnityEngine.Random.Range(minimum.y, maximum.y),
            UnityEngine.Random.Range(minimum.z, maximum.z)
        );
        StartCoroutine(gameSceneManager.FadeToBlackOutSquare(false, 2));
    }

    private void TruckHealthCheck()
    {
        if (lootTruck._truck.health <= 0)
        {
            HardPushEndGame();
        }
    }

    IEnumerator LootTruckHealthCheck()
    {
        while (true)
        {
            TruckHealthCheck();
            yield return new WaitForSeconds(2f);
        }
    }

    public void HardPushEndGame()
    {
        readyToStart = true;
    }

    private void Start()
    {
        _race = GetComponent<Race>();
    }
    public void PlayerCountDownCheck()
    {
        if (playerManager.connectedPlayers.Count >= m_iNumOfPlayersForGameStart)
        {
            readyToStart = true;
        }
    }
    private void Update()
    {
        //Debug.LogWarning("Room Game Start Time:" + _race._model.gameStartTime);
        //Debug.LogWarning("connected players:" + playerManager.connectedPlayers.Count);
        if (readyToStart && _race._model != null)
        {
            readyToStart = false;
            if (_race._model.gameStartTime == 0)
            {
                _race.ChangeGameTime(_realtime.room.time);
            }
            StartCoroutine(CountDownTimeContinously());
        }
    }
    private void DidDisconnectFromRoom(Realtime realtime)
    {
        chaseCam.ResetCam();
    }
    public void ConnectToRoom(int _selection)
    {
        switch (_selection)
        {
            case 1:
                preferredCar = "Car1";
                break;
            case 2:
                preferredCar = "Car2";
                break;
            case 3:
                preferredCar = "Car3";
                break;
            default:
                break;
        }
        if (playerNameInputField.text.Length > 0)
        {
            _realtime.Connect("UGP_TEST");
            StartCoroutine(gameSceneManager.FadeInAndOut(3, 1));
        }
    }
    private void DidConnectToRoom(Realtime realtime)
    {
        //isConnected = true;
        _tempName = preferredCar != "" ? preferredCar : "Car1";
        GameObject _temp = Realtime.Instantiate(_tempName,
                            position: spawnPoint,
                            rotation: Quaternion.identity,
                       ownedByClient: true,
            preventOwnershipTakeover: true,
                         useInstance: _realtime);

        if (_temp.GetComponent<NewCarController>()._realtime)
        {
            _temp.GetComponent<NewCarController>()._realtime = _realtime;
        }
        else
        {
            _temp.GetComponent<NewCarController>()._realtime = _realtime;
        }
        _temp.GetComponent<Player>().SetPlayerName(playerNameInputField.text);
        FindObjectOfType<MiniMapCamera>()._master = _temp.transform;
        _enterNameCanvas.gameObject.SetActive(false);
        _miniMapCamera.enabled = true;
        StartCoroutine(DelayPlayerCountCheck(5));
    }

    private IEnumerator DelayPlayerCountCheck(int DelayTime)
    {
        yield return new WaitForSeconds(DelayTime);
        //After delay time this is when the game has started
        lootTruck = FindObjectOfType<Truck>();
        playerManager.AddExistingPlayers();
        StartCoroutine(LootTruckHealthCheck());
        //PlayerCountDownCheck();
    }
    private IEnumerator CountDownTimeContinously()
    {
        while (true)
        {
            TimerCountDown();
            yield return null;
        }
    }
    private void TimerCountDown()
    {
        double _temp = _race.m_fRaceDuration - (_realtime.room.time - _race.m_fGameStartTime);
        //Debug.Log("Remaining Time: " + _temp);
        uIManager.remainingTime = _temp;
        //Update the timer for all managers instances
        if (_temp <= 0)
        {
            //SceneTransition Commence Logic should be here
            //Fade out of scene first
            StartCoroutine(gameSceneManager.FadeToBlackOutSquare(true, 1));
            StartCoroutine(gameSceneManager.DelaySceneTransiton(4f,
                SceneManager.GetActiveScene().buildIndex + 1));
        }
    }
}
public struct GameWinConditions
{
    //Parameters to fulfill winconditions
    public int winConIndex;
    public int playerIDtoAward;
    public bool isCompleted;
}

