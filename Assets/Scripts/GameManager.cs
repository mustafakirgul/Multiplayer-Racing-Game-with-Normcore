using Normal.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Realtime _realtime;
    public Vector3 center, size; //for player spawn box
    [Range(0, 359)] public float direction; //y angle of the spawned player
    Vector3 spawnPoint;

    [Space] [Space] [Header("UI and Camera")]
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

    [SerializeField] private Outline TruckOutline;

    public float MinXrayTruckOutlineDistance;

    [Space] [Space] [Header("Managers")]
    //Managers
    private PlayerManager playerManager;

    public UIManager uIManager;
    [SerializeField] private LootManager lootManager;
    [SerializeField] private GameSceneManager gameSceneManager;

    public PhaseManager phaseManager;

    public WallLocalMarker[] Walls;

    public bool truckIsKilled;
    public bool isHost;
    public GameObject gameCreationMenu;
    private JukeBox jukebox => FindObjectOfType<JukeBox>();
    public string _roomName;

    public bool isDebugBuild;
    public float debugTruckHealth;
    public string playerName;
    public Results results;
    public TopRacersLive trl;

    public bool GameStarted = false;
    [SerializeField] GameObject PanCamera;
    WaitForSeconds wait2secs;
    WaitForEndOfFrame waitFrame;
    public StartCountdown counter;
    [Space] [Header("Start Settings")] public float countdownBeforeStart = 10f;

    public NewCarController localController;

//all RIGOs (RIGO = Realtime Instantiated Game Object) will be recorded here, except for the projectiles and lobbiests.
    [Space]
    [Header("Settings for Realtime Instantiated GameObjects' Controls")]
    [Tooltip("RIGO = Realtime Instantiated Game Object")]
    public List<GameObject> RIGOs;

    public void RecordRIGO(GameObject RIGO)
    {
        if (RIGOs == null) RIGOs = new List<GameObject>();
        if (!RIGOs.Contains(RIGO)) RIGOs.Add(RIGO);
    }

    public void DestroyRIGOs()
    {
        for (int i = 0; i < RIGOs.Count; i++)
        {
            if (RIGOs[i] != null) Realtime.Destroy(RIGOs[i]);
        }

        RIGOs.Clear();
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
    }

    #endregion

    public void UpdateLootTruckTorqueFactor(float _f)
    {
        if (lootTruck != null)
        {
            lootTruck.UpdateTorqueFactor(_f);
        }
    }

    public void UpdateLootTruckHandBrakeState(bool _b)
    {
        if (lootTruck != null)
        {
            lootTruck.Handbrake(_b);
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

    private bool isFirstTime = true;

    private void Awake()
    {
        SingletonCheck();
        counter = FindObjectOfType<StartCountdown>();
        wait2secs = new WaitForSeconds(2f);
        waitFrame = new WaitForEndOfFrame();
        phaseManager = GetComponent<PhaseManager>();
        gameSceneManager = FindObjectOfType<GameSceneManager>();
        chaseCam = FindObjectOfType<ChaseCam>();
        playerManager = FindObjectOfType<PlayerManager>();
        uIManager = FindObjectOfType<UIManager>();
        lootManager = FindObjectOfType<LootManager>();
        _enterNameCanvas = GameObject.FindGameObjectWithTag("garage").GetComponent<Canvas>();
        _enterNameCanvas.gameObject.SetActive(true);
        //HeatMeter = GameObject.FindGameObjectWithTag("OverHeatMeter");
        //HeatText = GameObject.FindGameObjectWithTag("OverHeatText");
        // Get the Realtime component on this game object
        _realtime = GetComponent<Realtime>();
        spawnPoint =
            new Vector3( // this is just for the start, each car will move to a different spot before the race starts
                UnityEngine.Random.Range(center.x - (size.x * .5f), center.x + (size.x * .5f)),
                UnityEngine.Random.Range(center.y - (size.y * .5f), center.y + (size.y * .5f)),
                UnityEngine.Random.Range(center.z - (size.z * .5f), center.z + (size.z * .5f))
            );
        //StartCoroutine(gameSceneManager.FadeToBlackOutSquare(false, 1));
        if (gameCreationMenu == null) return;
        gameCreationMenu.SetActive(true);
    }

    private IEnumerator Start()
    {
        FixAssociations();
        yield return new WaitForSeconds(1f);
        jukebox.SwitchState(State.menu);
    }

    private void ResetBoolsForNewRound()
    {
        truckIsKilled = false;
        readyToStart = false;
        isCountingDown = false;
        GameStarted = false;

        if (lootTruck != null)
        {
            lootTruck.StartHealth();
        }
    }

    public void TruckHealthCheck()
    {
        if (lootTruck._health <= 0)
        {
            if (!truckIsKilled)
            {
                truckIsKilled = true;
                phaseManager.NextPhase();
                readyToStart = true;
                //Debug.LogWarning("IronHog has been killed!");
            }
        }
    }

    private IEnumerator CheckTruckDistanceOutline()
    {
        while (instance.gameObject.activeSelf)
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

            playerManager.UpdateExistingPlayers();
            yield return wait2secs;
        }
    }

    private void Update()
    {
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
                StartCoroutine(CountDownTimeContinuously());
            }
        }

        if (lootTruck != null)
            if (GameStarted) TruckHealthCheck();
            else lootTruck = FindObjectOfType<Truck>();
    }

    public void RaceEnded()
    {
        chaseCam.ResetCam();
    }

    public void StartTheRace(int _selection)
    {
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
        }

        LobbyManager.instance.SendData(_selection + 1);

        if (playerNameInputField.text.Length > 0)
        {
            _roomName = LobbyManager.instance.roomName;
            Cursor.visible = false;
            FixAssociations();
            var phaseManagerPhase = instance.phaseManager.phases[0];
            if (isFirstTime)
            {
                isFirstTime = false;
                phaseManagerPhase.duration = 16f;
            }
            else phaseManagerPhase.duration = 3f;

            instance.phaseManager.phases[0] = phaseManagerPhase;
            SpawnCar();
            uIManager.LootTruckInvincibleIcon.SetActive(false);
            localController.GetComponent<NewCarController>().PlaceCar();
        }
    }

    public void FixAssociations()
    {
        if (playerNameInputField == null)
        {
            playerNameInputField = GameObject.FindGameObjectWithTag("enterNameField").GetComponent<TextMeshProUGUI>();
        }

        gameSceneManager = FindObjectOfType<GameSceneManager>();
        chaseCam = FindObjectOfType<ChaseCam>();
        playerManager = FindObjectOfType<PlayerManager>();
        uIManager = FindObjectOfType<UIManager>();
        truckIsKilled = false;
    }

    private void SpawnCar()
    {
        _tempName = preferredCar != "" ? preferredCar : "Car1";
        GameObject _temp = Realtime.Instantiate(_tempName,
            position: spawnPoint,
            rotation: Quaternion.identity,
            ownedByClient: true,
            preventOwnershipTakeover: true,
            useInstance: _realtime);
        instance.RecordRIGO(_temp);
        playerName = playerNameInputField.text;

        _temp.GetComponent<Player>().SetPlayerName(playerName);
        localController = _temp.GetComponent<NewCarController>();
        ResetBoolsForNewRound();
        _enterNameCanvas = GameObject.FindGameObjectWithTag("garage").GetComponent<Canvas>();
        _enterNameCanvas.gameObject.SetActive(false);

        Walls = FindObjectsOfType<WallLocalMarker>(); // todo move to race start logic
        if (LobbyManager.instance.isHost) ResetWalls();

        PlayerManager.instance.AddLocalPlayer(_temp.transform);
        jukebox.SwitchState(State.game);
        _temp.transform.rotation = Quaternion.AngleAxis(PlayerManager.instance.directionOffset, Vector3.up);

        PanCamera.SetActive(true); //todo integrate into start sequence
        if (PanCamera.GetComponentInChildren<CameraMover>() != null)
            PanCamera.GetComponentInChildren<CameraMover>().StartMoving();
        phaseManager.StartPhaseSystem();
        StartCoroutine(CheckTruckDistanceOutline());
        if (LobbyManager.instance.isHost)
        {
            var _tempRace = Realtime.Instantiate("Race",
                position: transform.position,
                rotation: Quaternion.Euler(0, direction, 0),
                ownedByClient: true,
                preventOwnershipTakeover: true,
                useInstance: _realtime);
            _race = _tempRace.GetComponent<Race>();
            _race.ChangeIsOn(true);
        }
        else
        {
            _race = FindObjectOfType<Race>();
        }

        FindObjectOfType<CountdownLights>().Reset();
        StartCoroutine(WaitToSyncWeaponVisuals(_temp));
    }

    IEnumerator WaitToSyncWeaponVisuals(GameObject _temp)
    {
        yield return new WaitForSeconds(2f);
        lootTruck = FindObjectOfType<Truck>();
        TruckOutline = lootTruck.TruckOutline;
        //Vector3 CheckVector = new Vector3((_temp.GetComponent<ItemDataProcessor>().WeaponSelectorCount() - 1)
        //        , lootManager.VisualModelIndex().y, lootManager.VisualModelIndex().z);

        //Debug.Log("Check Vector is " + CheckVector);
        lootManager.ActivateVisualIndex();

        _temp.GetComponent<ItemDataProcessor>().ObtainLoadOutData(lootManager.ObtainCurrentBuild());
        _temp.GetComponent<ItemDataProcessor>().ProcessVisualIndices(false);
    }


    public void StartInitialCountdown()
    {
        counter.Initialize(_race);
    }

    private IEnumerator CountDownTimeContinuously()
    {
        while (instance.gameObject.activeSelf)
        {
            while (isCountingDown)
            {
                TimerCountDown();
                yield return waitFrame;
            }

            isCountingDown = false;
            yield return waitFrame;
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

    public void StartEndDisplaySequence()
    {
        if (GameManager.instance.isHost)
        {
            _race.ChangeIsOn(false);
        }

        StartCoroutine(EndDisplaySequence());
    }

    private IEnumerator EndDisplaySequence()
    {
        yield return StartCoroutine(gameSceneManager.FadeToBlackOutSquare(true, 2));
        GameSceneManager.instance.StartEndSplashes();
        Cursor.visible = true;
        yield return StartCoroutine(gameSceneManager.FadeToBlackOutSquare(false, 2));
    }

    private void ThingsToDoBeforeGameEnd()
    {
        //gather race information and store it for evaluation later
        //-----------------------------------------------------------
        if (results != null) results.PopulateList();

        //Loot manager will need to be update with new roles to do
        //Loot manager needs to know consumables/powerups like scrap that can persist

        //Disable other things that needs to be disabled in game
        uIManager.timeRemaining.ClearMesh();
        //Debug.LogWarning("HealthCheckStoppedAtTheEndOfTheGame");
        //reset lobby manager
        LobbyManager.instance.Reset();
        localController.ToggleController(false);
        RaceEnded();
    }

    public void Quit()
    {
        Application.Quit();
    }
}