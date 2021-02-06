using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Normal.Realtime;
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
    private Coroutine cr_RoomChecker;

    private bool isConnectedToALobby;
    private bool tooManyPLayers;
    private bool tryingToConnect;

    public float serverCheckDelay = 1f;
    public List<Lobbiest> lobbiests;

    private WaitForSeconds wait => new WaitForSeconds(serverCheckDelay);
    public Image radialLoader, feedbackLoader;
    public Text playerNumber, readyPlayerNumber, feedback;
    private string roomName;
    private UIManager uIManager;
    private Canvas canvas;
    public InputField roomNameCreate, roomNameJoin, numberOfPlayers;

    private void Awake()
    {
        lobbiests = new List<Lobbiest>();
        uIManager = FindObjectOfType<UIManager>();
        canvas = GetComponent<Canvas>();
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
                uIManager.ConnectToRoom(roomName);
        }
        else
        {
            radialLoader.fillAmount = 0f;
        }

        readyPlayerNumber.text = ready.ToString();
    }

    public void ConnectToLobby(bool create) // if create is false it will only try to connect
    {
        isHost = create;
        tryingToConnect = true;
        if (create)
        {
            if (roomNameCreate.text.Length == 0)
            {
                feedback.text += "Room name cannot be blank\n";
                return; //name cannot be blank
            }

            roomName = roomNameCreate.text;
            feedback.text += "Trying to connect: " + roomName + "\n";

            if (numberOfPlayers.text.Length != 1)
            {
                feedback.text += "Maximum player number must be single digit\n";
                return; //not a one digit number
            }

            if (!int.TryParse(numberOfPlayers.text, out maxPlayers))
            {
                feedback.text += "'" + numberOfPlayers.text + "' is not a number\n";
                return;
            }

            feedback.text += "Max Player number is set to " + maxPlayers + "\n";
        }
        else
        {
            if (roomNameJoin.text.Length == 0)
            {
                feedback.text += "Room name cannot be blank\n";
                return; //name cannot be blank
            }

            roomName = roomNameJoin.text;
            feedback.text += "Trying to connect to room " + roomName + "\n";
        }

        _realtime.didConnectToRoom += DidConnectToLobby;
        _realtime.didDisconnectFromRoom += DidDisconnectFromLobby;
        _realtime.Connect(roomName + "_L088Y");
    }

    private void OnDestroy()
    {
        _realtime.didConnectToRoom += DidConnectToLobby;
        _realtime.didDisconnectFromRoom += DidDisconnectFromLobby;
    }

    public void DisconnectFromLobby()
    {
        if (_realtime.connected)
        {
            _realtime.Disconnect();
        }
    }

    public void MarkPlayerReady()
    {
        if (_lobbiest == null) return;
        _lobbiest.ChangeIsReady(true);
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
            useInstance: _realtime);
        _lobbiest = _temp.GetComponent<Lobbiest>();
        if (cr_RoomChecker != null) StopCoroutine(cr_RoomChecker);
        cr_RoomChecker = StartCoroutine(CR_Checkroom());
    }

    private IEnumerator CR_Checkroom()
    {
        yield return wait;
        var count = FindObjectsOfType<Lobbiest>().Length;
        if (!isHost)
        {
            if (count < 2)
            {
                feedback.text += "This room does not exist!!! Try creating one\n";
                DisconnectFromLobby();
            }
            else
            {
                foreach (var l in lobbiests.Where(l => l.isHost))
                {
                    maxPlayers = l.maxPlayers;
                }
            }
        }

        _lobbiest.ChangeMaxPlayers(maxPlayers);

        if (count > maxPlayers)
        {
            feedback.text += "Too Many Players!!! Max players for this room is limited to " + maxPlayers +
                             " but you are number " + count + "\n";
            DisconnectFromLobby();
        }
        else
        {
            feedback.text += "Connected to lobby of " + roomName + "\n";
            isConnectedToALobby = true;
            _lobbiest.ChangeIsHost(isHost);
            _lobbiest.ChangeRoomName(roomName);
            canvas.enabled = false;
        }

        cr_RoomChecker = null;
        tryingToConnect = false;
    }

    void DidDisconnectFromLobby(Realtime realtime)
    {
        feedback.text = "";
        isConnectedToALobby = false;
        lobbiests.Clear();
        _realtime.didConnectToRoom -= DidConnectToLobby;
        _realtime.didDisconnectFromRoom -= DidDisconnectFromLobby;
    }
}