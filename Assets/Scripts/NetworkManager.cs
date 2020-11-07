using Normal.Realtime;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private Realtime _realtime;
    public Vector3 minimum, maximum;
    Vector3 spawnPoint;
    public TextMeshProUGUI playerNameInputField;
    public Canvas _enterNameCanvas;
    public Camera _miniMapCamera;

    private void Awake()
    {
        _enterNameCanvas.gameObject.SetActive(true);
        // Get the Realtime component on this game object
        _realtime = GetComponent<Realtime>();

        // Notify us when Realtime successfully connects to the room
        _realtime.didConnectToRoom += DidConnectToRoom;

        spawnPoint = new Vector3(
            Random.Range(minimum.x,maximum.x), 
            Random.Range(minimum.y, maximum.y), 
            Random.Range(minimum.z, maximum.z)
            );
    }

    public void ConnectToRoom()
    {
        if (playerNameInputField.text.Length>0)
        {
            _realtime.Connect("UGP_Test");
        }
    }
    private void DidConnectToRoom(Realtime realtime)
    {
        // Instantiate the CubePlayer for this client once we've successfully connected to the room
        GameObject _temp = Realtime.Instantiate("Car",                 // Prefab name
                            position: spawnPoint,          // Start 1 meter in the air
                            rotation: Quaternion.identity, // No rotation
                       ownedByClient: true,                // Make sure the RealtimeView on this prefab is owned by this client
            preventOwnershipTakeover: true,                // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                         useInstance: _realtime);           // Use the instance of Realtime that fired the didConnectToRoom event.

        _temp.GetComponent<WC_Car_Controller>()._realtime = _realtime;
        _temp.GetComponent<Player>().SetPlayerName(playerNameInputField.text);
        FindObjectOfType<MiniMapCamera>()._master = _temp.transform;
        _enterNameCanvas.gameObject.SetActive(false);
        _miniMapCamera.enabled = true;
    }
}
