using Normal.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private Realtime _realtime;
    public Vector3 minimum, maximum;
    Vector3 spawnPoint;



    private void Awake()
    {
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

    private void DidConnectToRoom(Realtime realtime)
    {
        // Instantiate the CubePlayer for this client once we've successfully connected to the room
        Realtime.Instantiate("Car",                 // Prefab name
                            position: spawnPoint,          // Start 1 meter in the air
                            rotation: Quaternion.identity, // No rotation
                       ownedByClient: true,                // Make sure the RealtimeView on this prefab is owned by this client
            preventOwnershipTakeover: true,                // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                         useInstance: realtime);           // Use the instance of Realtime that fired the didConnectToRoom event.
    }
}
