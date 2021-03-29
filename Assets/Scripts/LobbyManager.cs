using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Normal.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public bool isHost;
    public int maxPlayers = 8;
    private Lobbiest _lobbiest;
    private Realtime _realtime => FindObjectOfType<Realtime>();
    private RealtimeView _rtView => GetComponent<RealtimeView>();
    private Coroutine cr_RoomChecker;

    private bool isConnectedToALobby;
    private bool tooManyPLayers;
    private bool tryingToConnect;

    public float serverCheckDelay = 1f;
    public List<Lobbiest> lobbiests;

    private WaitForSeconds wait => new WaitForSeconds(serverCheckDelay);
    public Image radialLoader, feedbackLoader;
    public Text playerNumber, readyPlayerNumber, feedback, maxPlayerNumber, garageGoFeedback;
    private string roomName;
    private UIManager uIManager;
    private Canvas canvas;
    public InputField roomNameCreate, roomNameJoin, numberOfPlayers;
    public TMP_InputField playerNameInputField;
    private RectTransform feedbackLoaderRectTransform;
    private Coroutine cr_ConnectToRoom;
    private bool stayDisconnected = true;
    private JukeBox jukebox => FindObjectOfType<JukeBox>();
    private bool freshLobby = true;
    public Image readyLight;
    
    private void Awake()
    {
        lobbiests = new List<Lobbiest>();
        uIManager = FindObjectOfType<UIManager>();
        canvas = GetComponent<Canvas>();
        feedbackLoaderRectTransform = feedbackLoader.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        if (isConnectedToALobby)
        {
            LobbyLogic();
        }

        feedbackLoader.fillAmount = tryingToConnect ? Time.timeSinceLevelLoad % 1f : 0f;
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
                if (cr_ConnectToRoom != null)
                    StopCoroutine(cr_ConnectToRoom);
                float delay = 0f;
                if (!isHost)
                    delay = 1f;
                //Debug.LogWarning("Count: " + count + " | Ready: " + ready);
                cr_ConnectToRoom = StartCoroutine(CR_ConnectToRace(delay));
            }
        }
        else
        {
            radialLoader.fillAmount = 0f;
        }

        readyPlayerNumber.text = ready.ToString();
        maxPlayerNumber.text = "/ " + maxPlayers;
    }

    IEnumerator CR_ConnectToRace(float delay)
    {
        yield return new WaitForSeconds(delay);
        _realtime.Disconnect();
    }

    public void ConnectToLobby(bool create) // if create is false it will only try to connect an existing room
    {
        if (_realtime.connected)
        {
            _realtime.Disconnect();
        }

        radialLoader.fillAmount = 0;

        isHost = create;
        GameManager.instance.isHost = isHost;
        tryingToConnect = true;
        if (create)
        {
            if (roomNameCreate.text.Length == 0)
            {
                feedback.text += "Room name cannot be blank\n";
                tryingToConnect = false;
                return; //name cannot be blank
            }

            roomName = roomNameCreate.text;
            feedback.text += "Trying to connect: " + roomName + "\n";

            if (numberOfPlayers.text.Length != 1)
            {
                feedback.text += "Maximum player number must be single digit\n";
                tryingToConnect = false;
                return; //not a one digit number
            }

            if (!int.TryParse(numberOfPlayers.text, out maxPlayers))
            {
                feedback.text += "'" + numberOfPlayers.text + "' is not a number\n";
                tryingToConnect = false;
                return;
            }
        }
        else
        {
            if (roomNameJoin.text.Length == 0)
            {
                feedback.text += "Room name cannot be blank\n";
                tryingToConnect = false;
                return; //name cannot be blank
            }

            roomName = roomNameJoin.text.ToLower();
            feedback.text += "Trying to connect to room " + roomName + "\n";
        }

        _realtime.didConnectToRoom += DidConnectToLobby;
        _realtime.didDisconnectFromRoom += DidDisconnectFromLobby;
        GameManager.instance._roomName = roomName;
        _realtime.Connect(roomName);
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

        stayDisconnected = false;
        readyLight.enabled = true;
        _lobbiest.ChangeIsReady(true);
    }

    public void ClearGoFeedback()
    {
        if (playerNameInputField.text.Length > 0) garageGoFeedback.text = "";
    }

    public string RoomName()
    {
        return _lobbiest.roomName;
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

    void DidConnectToLobby(Realtime realtime)
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
    }

    private IEnumerator CR_Checkroom()
    {
        yield return wait;
        _lobbiest.ChangeMaxPlayers(maxPlayers);
        var count = lobbiests.Count;
        if (freshLobby)
        {
            freshLobby = false;
            if (!isHost) //if not creating but just trying to join
            {
                if (count == 1) // there is only you in the room
                {
                    feedback.text += "This room does not exist!!! Try creating one\n";
                    stayDisconnected = true;
                    DisconnectFromLobby();
                    yield break;
                }

                for (int i = 0; i < lobbiests.Count; i++)
                {
                    if (lobbiests[i].isHost)
                        maxPlayers = lobbiests[i].maxPlayers;
                }
            }
            else
            {
                if (GameManager.instance._race.m_isOn)
                {
                    feedback.text += "The race on this room has already started. Please create another room.";
                    stayDisconnected = true;
                    DisconnectFromLobby();
                    yield break;
                }

                if (count > 1)
                {
                    feedback.text +=
                        "A room with this name has already been created. Please try creating another room with a different name.";
                    stayDisconnected = true;
                    DisconnectFromLobby();
                    yield break;
                }
            }

            if (count > maxPlayers && maxPlayers > 0)
            {
                feedback.text += "Too Many Players!!! Max players for this room is limited to " + maxPlayers +
                                 " and you are number " + count + "\n";
                stayDisconnected = true;
                DisconnectFromLobby();
                yield break;
            }
        }

        feedback.text += "Connected to lobby of " + roomName + "\n";
        canvas.enabled = false;
        stayDisconnected = true;
        feedback.text = "";
        cr_RoomChecker = null;
        yield return new WaitForSeconds(2f);
        isConnectedToALobby = true;
        tryingToConnect = false;
        feedback.text += "Max Player number is set to " + maxPlayers + "\n";
    }

    void DidDisconnectFromLobby(Realtime realtime)
    {
        isConnectedToALobby = false;
        lobbiests.Clear();
        _realtime.didConnectToRoom -= DidConnectToLobby;
        _realtime.didDisconnectFromRoom -= DidDisconnectFromLobby;
        if (stayDisconnected) return;
        uIManager.ConnectToRoom();
    }
}