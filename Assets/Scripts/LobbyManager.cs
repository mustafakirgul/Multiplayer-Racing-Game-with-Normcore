using System.Collections;
using System.Collections.Generic;
using System.Text;
using Normal.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public bool isHost;
    private readonly int maxPlayers = 9;
    public Lobbiest _lobbiest;
    private Realtime _realtime => FindObjectOfType<Realtime>();
    private Coroutine cr_RoomChecker;

    private bool isConnectedToALobby;
    private bool tryingToConnect;

    public float serverCheckDelay = 1f;
    public List<Lobbiest> lobbiests;

    private WaitForSeconds wait => new WaitForSeconds(serverCheckDelay);
    public Image radialLoader, feedbackLoader;
    public Text playerNumber, readyPlayerNumber, feedback, maxPlayerNumber, garageGoFeedback;
    public string roomName;
    private UIManager uIManager;
    public GameObject connectionPanel;
    public InputField roomNameJoin;
    public TMP_InputField playerNameInputField;
    private RectTransform feedbackLoaderRectTransform;
    private JukeBox jukebox => FindObjectOfType<JukeBox>();
    public Image readyLight;
    [Space] public char[] characters;
    public Text roomNameBanner;
    public string statisticsURL;
    private int userId;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CheckUserID();
        lobbiests = new List<Lobbiest>();
        uIManager = FindObjectOfType<UIManager>();
        feedbackLoaderRectTransform = feedbackLoader.GetComponent<RectTransform>();
    }

    private void CheckUserID()
    {
        if (PlayerPrefs.HasKey("USERID"))
            userId = PlayerPrefs.GetInt("USERID");
        else
        {
            userId = GenerateUserID();
            PlayerPrefs.SetInt("USERID", userId);
        }
    }

    private int GenerateUserID()
    {
        return
            Random.Range(1, 9) * 10000000 +
            Random.Range(1, 9) * 1000000 +
            Random.Range(1, 9) * 100000 +
            Random.Range(1, 9) * 10000 +
            Random.Range(1, 9) * 1000 +
            Random.Range(1, 9) * 100 +
            Random.Range(1, 9) * 10 +
            Random.Range(1, 9);
    }


    public void JoinRoom()
    {
        radialLoader.fillAmount = 0;
        isHost = false;
        GameManager.instance.isHost = isHost;
        tryingToConnect = true;
        roomName = roomNameJoin.text;
        //IF ROOMNAME IS EMPTY THEN DO STUFF, ELSE
        if (roomName.Length == 0)
            feedback.text += "Room name cannot be blank!\n";
        else
            feedback.text += "Connecting to room: " + roomName + "\n";
        Debug.LogWarning("ROOM: " + roomName);
        GameManager.instance._roomName = roomName;
        roomNameBanner.text = roomName;
        _realtime.didConnectToRoom += DidConnectToGame;
        _realtime.didDisconnectFromRoom += DidDisconnectFromGame;
        _realtime.Connect(roomName);
    }

    private void Update()
    {
        if (isConnectedToALobby)
        {
            LobbyLogic();
        }

        feedbackLoader.fillAmount = tryingToConnect ? Time.timeSinceLevelLoad % 1f : 0f;
        if (feedbackLoaderRectTransform == null) return;
        feedbackLoaderRectTransform.localEulerAngles = tryingToConnect
            ? new Vector3(feedbackLoaderRectTransform.localEulerAngles.x,
                feedbackLoaderRectTransform.localEulerAngles.y,
                feedbackLoaderRectTransform.localEulerAngles.z + (Time.deltaTime * 180f))
            : Vector3.zero;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void LobbyLogic()
    {
        int count = lobbiests.Count;
        playerNumber.text = count.ToString();
        int ready = 0;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (lobbiests[i].isReady) ready++;
            }

            radialLoader.fillAmount = (ready * 1f) / (count * 1f);
            if (count == ready)
            {
                isConnectedToALobby = false;
                uIManager.ConnectToRoom();
            }
        }
        else
        {
            radialLoader.fillAmount = 0f;
        }

        readyPlayerNumber.text = ready.ToString();
        maxPlayerNumber.text = "/ " + maxPlayers;
    }

    public void CreateGame() // if create is false it will only try to connect an existing room
    {
        radialLoader.fillAmount = 0;

        isHost = true;
        GameManager.instance.isHost = isHost;
        tryingToConnect = true;

        roomName = GenerateRandomString(6);
        Debug.LogWarning("ROOM: " + roomName);
        GUIUtility.systemCopyBuffer = roomName;
        GameManager.instance._roomName = roomName;
        roomNameBanner.text = roomName;
        _realtime.didConnectToRoom += DidConnectToGame;
        _realtime.didDisconnectFromRoom += DidDisconnectFromGame;

        _realtime.Connect(roomName);
    }

    private string GenerateRandomString(int len)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            sb.Append(characters[Random.Range(0, characters.Length)]);
        }

        return sb.ToString();
    }

    public void DisconnectFromLobby()
    {
        if (_realtime.connected)
        {
            _realtime.Disconnect();
        }

        tryingToConnect = false;
    }

    public void MarkPlayerReady()
    {
        if (_lobbiest == null) return;
        int nameSize = playerNameInputField.text.Length;
        if (nameSize < 1)
        {
            garageGoFeedback.text = "Name cannot be blank!\n";
            return;
        }

        if (nameSize > 10)
        {
            garageGoFeedback.text = "Name size limit is 10 chars!\n";
            return;
        }

        readyLight.enabled = true;
        _lobbiest.ChangeIsReady(true);
    }

    public void Reset()
    {
        jukebox.SwitchState(State.menu);
        _lobbiest.ChangeIsReady(false);
        readyLight.enabled = false;
        tryingToConnect = false;
        isConnectedToALobby = true;
        garageGoFeedback.text = "";
    }

    public void ClearGoFeedback()
    {
        if (playerNameInputField.text.Length > 0) garageGoFeedback.text = "";
    }

    public void RegisterLobbiest(Lobbiest lobbiest)
    {
        if (lobbiests.Contains(lobbiest)) return;
        lobbiests.Add(lobbiest);
    }

    public void RemoveLobbiest(Lobbiest lobbiest)
    {
        if (lobbiests.Contains(lobbiest))
            lobbiests.Remove(lobbiest);
    }

    void DidConnectToGame(Realtime realtime)
    {
        GameObject _temp = Realtime.Instantiate("Lobbiest",
            position: Vector3.zero,
            rotation: Quaternion.identity,
            ownedByClient: true,
            preventOwnershipTakeover: true,
            destroyWhenOwnerOrLastClientLeaves: true,
            useInstance: _realtime);
        _lobbiest = _temp.GetComponent<Lobbiest>();
        _lobbiest.ChangeIsHost(isHost);
        _lobbiest.ChangeRoomName(roomName);
        if (cr_RoomChecker != null) StopCoroutine(cr_RoomChecker);
        cr_RoomChecker = StartCoroutine(CR_Checkroom());
        jukebox.SwitchState(State.menu);
        readyLight.enabled = false;
        tryingToConnect = false;
    }

    private IEnumerator CR_Checkroom()
    {
        yield return wait;
        _lobbiest.ChangeMaxPlayers(maxPlayers);
        var count = lobbiests.Count;
        if (GameManager.instance._race == null) GameManager.instance._race = FindObjectOfType<Race>();
        if (GameManager.instance._race != null)
        {
            if (GameManager.instance._race.m_isOn)
            {
                feedback.text += "The race on this room has already started. Please create another room.";
                DisconnectFromLobby();
                yield break;
            }
        }

        if (count > maxPlayers && maxPlayers > 0)
        {
            feedback.text += "Too Many Players!!! Max players for this room is limited to " + maxPlayers +
                             " and you are number " + count + "\n";
            DisconnectFromLobby();
            yield break;
        }

        if (count == 1 && !isHost)
        {
            feedback.text += "this room does not exist! " + "\n";
            DisconnectFromLobby();
            yield break;
        }

        feedback.text += "Connected to room " + roomName + "\n";
        connectionPanel.SetActive(false);
        feedback.text = "";

        yield return new WaitForSeconds(2f);
        isConnectedToALobby = true;
        tryingToConnect = false;
        cr_RoomChecker = null;
    }

    void DidDisconnectFromGame(Realtime realtime)
    {
        isConnectedToALobby = false;
        _realtime.didConnectToRoom -= DidConnectToGame;
        _realtime.didDisconnectFromRoom -= DidDisconnectFromGame;
    }

    public void SendData(int buildNo)
    {
        StartCoroutine(CR_SendData(buildNo));
    }

    IEnumerator CR_SendData(int buildNo)
    {
        string uRL = statisticsURL + "?userId=" + userId + "&odaId=" + roomName + "&status=" +
                     (isHost ? "host" : "user") + "&car=" + buildNo;
        //Debug.Log("REQUEST TRACKABLES FOR RESEARCH " + researchID + " ON: " + uRL);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uRL))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Data send error!");
            }
            else
            {
                Debug.Log("Data sent!");
            }
        }

        yield return null;
    }
}