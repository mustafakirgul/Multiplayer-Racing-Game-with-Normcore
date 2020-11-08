using UnityEngine;

public class PropTruck : MonoBehaviour
{
    public bool registerAsNetworkPlayer = true;

    private void Start()
    {
        if (registerAsNetworkPlayer)
        {
            PlayerManager.instance.AddNetworkPlayer(transform);
        }
    }
}
