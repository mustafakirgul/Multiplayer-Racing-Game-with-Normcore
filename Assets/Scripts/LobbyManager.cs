using System.Collections.Generic;
using System.Linq;
using Normal.Realtime;
using UnityEngine;
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

    //public float serverCheckDelay;
    public List<Lobbiest> lobbiests;

    //private WaitForSeconds wait => new WaitForSeconds(serverCheckDelay);
    public Image radialLoader;
    public Text playerNumber, readyPlayerNumber;
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
        if (create)
        {
            isHost = true;
            if (roomNameCreate.text.Length == 0)
            {
                Debug.LogWarning("Room name cannot be blank");
                return; //name cannot be blank
            }

            roomName = roomNameCreate.text;
            Debug.LogWarning("Trying to connect: " + roomName);

            if (numberOfPlayers.text.Length != 1)
            {
                Debug.LogWarning("There is more than 1 digit");
                return; //not a one digit number
            }

            if (!int.TryParse(numberOfPlayers.text, out maxPlayers))
            {
                Debug.LogWarning("Please enter a number for max number of players");
                return;
            }

            Debug.LogWarning("Max Player number is: " + maxPlayers);
        }
        else
        {
            if (roomNameJoin.text.Length == 0)
            {
                Debug.LogWarning("Room name cannot be blank");
                return; //name cannot be blank
            }

            roomName = roomNameJoin.text;
            Debug.LogWarning("Trying to connect: " + roomName);
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
        _lobbiest.ChangeRoomName(roomName);
        _lobbiest.ChangeMaxPlayers(maxPlayers);
        var count = FindObjectsOfType<Lobbiest>().Length;
        if (!isHost)
        {
            foreach (var l in lobbiests.Where(l => l.isHost))
            {
                maxPlayers = l.maxPlayers;
            }
        }

        _lobbiest.ChangeMaxPlayers(maxPlayers);

        if (count > maxPlayers)
        {
            Debug.LogWarning("TooManyPlayers!!!");
            DisconnectFromLobby();
        }
        else
        {
            Debug.LogWarning("Connected to lobby.");
            isConnectedToALobby = true;
            _lobbiest.ChangeIsHost(isHost);
            canvas.enabled = false;
        }
    }

    void DidDisconnectFromLobby(Realtime realtime)
    {
        isConnectedToALobby = false;
        lobbiests.Clear();
        _realtime.didConnectToRoom -= DidConnectToLobby;
        _realtime.didDisconnectFromRoom -= DidDisconnectFromLobby;
        Realtime.Destroy(_lobbiest.gameObject);
    }
}