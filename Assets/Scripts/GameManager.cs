﻿using Normal.Realtime;
using TMPro;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Realtime _realtime;
    public Vector3 center, size; //for player spawn box
    [Range(0, 359)] public float direction; //y angle of the spawned player
    Vector3 spawnPoint;

    [Space]
    [Space]
    [Header("UI and Camera")]
    public TextMeshProUGUI playerNameInputField;

    public Canvas _enterNameCanvas;
    public GameObject HeatMeter;
    public GameObject HeatText;

    public Camera _miniMapCamera;
    public string preferredCar;
    string _tempName;
    [SerializeField] ChaseCam chaseCam;

    //Game Manager Params
    public float m_fMaxTimer;
    public float m_localTimer;
    public int m_iNumOfPlayersForGameStart;
    public Race _race;
    public bool readyToStart;
    public Truck lootTruck;

    [SerializeField]
    private Outline TruckOutline;

    public float MinXrayTruckOutlineDistance;

    [Space]
    [Space]
    [Header("Managers")]
    //Managers
    private PlayerManager playerManager;

    public UIManager uIManager;
    [SerializeField] private LootManager lootManager;
    [SerializeField] private GameSceneManager gameSceneManager;

    public PhaseManager phaseManager;

    public WallLocalMarker[] Walls;

    public bool truckIsKilled;

    private void OnDrawGizmos()
    {
        float radians = direction * Mathf.Deg2Rad;
        float x = Mathf.Cos(radians);
        float y = Mathf.Sin(radians);
        Vector3 pos = new Vector3(x, 0, y); //Vector2 is fine, if you're in 2D
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + (pos * 5f));
    }

    #region Singleton Logic

    public static GameManager instance = null;
    private bool isCountingDown;

    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    public void UpdateLootTruckTorqueFactor(float _f)
    {
        if (lootTruck != null)
        {
            lootTruck.UpdateToqueFactor(_f);
        }
    }

    public void UpdateLootTruckHandBrakeState(bool _b)
    {
        if (lootTruck != null)
        {
            lootTruck.Handrake(_b);
        }
    }

    public void UpdateInvincibilityOfTruck(bool _i)
    {
        if (lootTruck != null)
        {
            lootTruck.SetInvincibility(_i);
        }
    }

    public void ResetWalls()
    {
        for (int i = 0; i < Walls.Length; i++)
        {
            Walls[i].ResetWall();
        }
    }

    public void MakeWallsGoUp()
    {
        for (int i = 0; i < Walls.Length; i++)
        {
            Walls[i].CloseWall();
        }
    }

    public void MakeWallsGoDown()
    {
        for (int i = 0; i < Walls.Length; i++)
        {
            Walls[i].OpenWall();
        }
    }

    private void Awake()
    {
        SingletonCheck();
        phaseManager = GetComponent<PhaseManager>();
        gameSceneManager = FindObjectOfType<GameSceneManager>();
        chaseCam = GameObject.FindObjectOfType<ChaseCam>();
        playerManager = FindObjectOfType<PlayerManager>();
        uIManager = FindObjectOfType<UIManager>();
        lootManager = FindObjectOfType<LootManager>();
        _enterNameCanvas.gameObject.SetActive(true);
        //HeatMeter = GameObject.FindGameObjectWithTag("OverHeatMeter");
        //HeatText = GameObject.FindGameObjectWithTag("OverHeatText");
        // Get the Realtime component on this game object
        _realtime = GetComponent<Realtime>();

        // Notify us when Realtime successfully connects to the room
        _realtime.didConnectToRoom += DidConnectToRoom;
        _realtime.didDisconnectFromRoom += DidDisconnectFromRoom;
        spawnPoint = new Vector3(
            UnityEngine.Random.Range(center.x - (size.x * .5f), center.x + (size.x * .5f)),
            UnityEngine.Random.Range(center.y - (size.y * .5f), center.y + (size.y * .5f)),
            UnityEngine.Random.Range(center.z - (size.z * .5f), center.z + (size.z * .5f))
        );
        //StartCoroutine(gameSceneManager.FadeToBlackOutSquare(false, 1));
    }

    private void ResetBoolsForNewRound()
    {
        truckIsKilled = false;
        readyToStart = false;
        isCountingDown = false;

        if (lootTruck != null)
        {
            lootTruck.StartHealth();
        }
    }

    public void TruckHealthCheck()
    {
        if (lootTruck._health <= 0)
        {
            if (!GameManager.instance.truckIsKilled)
            {
                GameManager.instance.truckIsKilled = true;
                GameManager.instance.readyToStart = true;
                GameManager.instance.phaseManager.NextPhase();
                //Debug.LogWarning("IronHog has been killed!");
            }
        }
    }

    private void Start()
    {
        _race = GetComponent<Race>();
        Walls = FindObjectsOfType<WallLocalMarker>();
    }

    /*public void PlayerCountDownCheck()
    {
        if (playerManager.connectedPlayers.Count >= m_iNumOfPlayersForGameStart)
        {
            readyToStart = true;
        }
    }*/


    private IEnumerator CheckTruckDistanceOutline()
    {
        while (true)
        {
            if (lootTruck != null &&
                PlayerManager.instance.localPlayer != null)
            {
                if (Vector3.Distance(lootTruck.transform.position,
                    PlayerManager.instance.localPlayer.transform.position) > MinXrayTruckOutlineDistance)
                {
                    TruckOutline.enabled = true;
                }
                else
                {
                    TruckOutline.enabled = false;
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }
    private void Update()
    {
        //Debug.LogWarning("Room Game Start Time:" + _race._model.gameStartTime);
        //Debug.LogWarning("connected players:" + playerManager.connectedPlayers.Count);
        if (readyToStart && _race != null)
        {
            readyToStart = false;
            if (_race.m_fGameStartTime == 0)
            {
                _race.ChangeGameTime(_realtime.room.time);
            }

            if (!isCountingDown)
            {
                isCountingDown = true;
                StartCoroutine(CountDownTimeContinously());
            }
        }

        if (lootTruck != null)
        {
            TruckHealthCheck();
        }
    }

    private void DidDisconnectFromRoom(Realtime realtime)
    {
        chaseCam.ResetCam();
    }

    public void ConnectToRoom(int _selection)
    {
        if (_realtime.connected)
        {
            _realtime.Disconnect();
        }

        switch (_selection)
        {
            case 0:
                preferredCar = "Car1";
                break;
            case 1:
                preferredCar = "Car2";
                break;
            case 2:
                preferredCar = "Car3";
                break;
            default:
                break;
        }

        if (playerNameInputField.text.Length > 0)
        {
            Debug.Log(preferredCar);
            _realtime.Connect("UGP_TEST5");

            //Coroutines tend to mess up here due to connection/network related
            //issues
            //StartCoroutine(gameSceneManager.FadeToBlackOutSquare(true, 1));
        }
    }

    public void FixAssociations()
    {
        playerNameInputField = GameObject.FindGameObjectWithTag("enterNameField").GetComponent<TextMeshProUGUI>();
        _enterNameCanvas = GameObject.FindGameObjectWithTag("enterNameCanvas").GetComponent<Canvas>();
        HeatMeter = GameObject.FindGameObjectWithTag("OverHeatMeter");
        HeatText = GameObject.FindGameObjectWithTag("OverHeatText");
        _miniMapCamera = GameObject.FindGameObjectWithTag("miniMapCamera").GetComponent<Camera>();

        gameSceneManager = FindObjectOfType<GameSceneManager>();
        chaseCam = GameObject.FindObjectOfType<ChaseCam>();
        playerManager = FindObjectOfType<PlayerManager>();
        uIManager = FindObjectOfType<UIManager>();
        truckIsKilled = false;
    }

    private void DidConnectToRoom(Realtime realtime)
    {
        //isConnected = true;
        _tempName = preferredCar != "" ? preferredCar : "Car1";
        GameObject _temp = Realtime.Instantiate(_tempName,
            position: spawnPoint,
            rotation: Quaternion.Euler(0, direction, 0),
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
        _temp.GetComponent<ItemDataProcessor>().ObtainLoadOutData(lootManager.ObatinCurrentBuild());
        FindObjectOfType<MiniMapCamera>()._master = _temp.transform;
        ResetBoolsForNewRound();
        _enterNameCanvas.gameObject.SetActive(false);
        _temp.GetComponent<NewCarController>().OverheatMeterObj.SetActive(false);
        _temp.GetComponent<NewCarController>().OverHeatNotice.SetActive(false);
        //HeatText.SetActive(false);
        Invoke("KeepTrackOfWinConditions", 3);
        //StartCoroutine(gameSceneManager.FadeToBlackOutSquare(false, 1));
        ResetWalls();
    }

    private void KeepTrackOfWinConditions()
    {
        if (lootTruck == null)
        {
            lootTruck = FindObjectOfType<Truck>();
            TruckOutline = lootTruck.TruckOutline;
            StartCoroutine(CheckTruckDistanceOutline());
            //TruckOutline.enabled = false;
        }

        playerManager.UpdateExistingPlayers();
        phaseManager.StartPhaseSystem();

        //TruckHealthCheckCR = StartCoroutine(LootTruckHealthCheck());
        //Debug.LogWarning("HealthCheckStartedAtTheBeginningOfTheGame");
    }

    private IEnumerator CountDownTimeContinously()
    {
        while (true)
        {
            while (isCountingDown)
            {
                TimerCountDown();
                yield return null;
            }

            isCountingDown = false;
            yield return null;
        }
    }

    private void TimerCountDown()
    {
        double _temp = _race.m_fRaceDuration - (_realtime.room.time - _race.m_fGameStartTime);
        //Debug.Log("Remaining Time: " + _temp);
        if (_temp > 0f)
        {
            uIManager.timeRemaining.ClearMesh();
            uIManager.timeRemaining.SetText(_temp.ToString("F2"));
        }

        //Update the timer for all managers instances
        if (_temp <= 0)
        {
            //SceneTransition Commence Logic should be here
            //Fade out of scene first
            //StartCoroutine(gameSceneManager.FadeToBlackOutSquare(true, 2));
            isCountingDown = false;

            ThingsToDoBeforeGameEnd();

            //Enable End Game Screens
            //StartCoroutine(GameSceneManager.instance.FadeInAndOut(2, 2, 3));
        }
    }

    public void DisconnectFromServer()
    {
        _realtime.room.Disconnect();
    }

    public void StartEndDisplaySequence()
    {
        StartCoroutine(EndDisplaySequence());
    }

    private IEnumerator EndDisplaySequence()
    {
        yield return StartCoroutine(gameSceneManager.FadeToBlackOutSquare(true, 2));
        GameSceneManager.instance.EnableSplashes(
            GameSceneManager.instance.GameEndSplashes);
        yield return StartCoroutine(gameSceneManager.FadeToBlackOutSquare(false, 2));
        lootManager.RollForLoot();
        yield return StartCoroutine(
            gameSceneManager.DelaySceneTransiton(3f)
        );
    }

    private void ThingsToDoBeforeGameEnd()
    {
        //gather race information and store it for evaluation later
        //-----------------------------------------------------------


        //Loot manager will need to be update with new roles to do
        //Loot manager needs to know consumables/powerups like scrap that can persist

        //Disable other things that needs to be disabled in game
        uIManager.timeRemaining.ClearMesh();
        //Debug.LogWarning("HealthCheckStoppedAtTheEndOfTheGame");
        _race.ChangeGameTime(0);
    }
}

public struct GameWinConditions
{
    //Parameters to fulfill winconditions
    public int winConIndex;
    public int playerIDtoAward;
    public bool isCompleted;
}