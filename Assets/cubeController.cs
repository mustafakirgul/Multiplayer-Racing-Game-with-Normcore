using Normal.Realtime;
using UnityEngine;

public class cubeController : MonoBehaviour
{
    RealtimeView _rV;
    RealtimeTransform _rT;

    private void Start()
    {
        _rV = GetComponent<RealtimeView>();
        _rT = GetComponent<RealtimeTransform>();
    }

    private void Update()
    {
        if (_rV.isOwnedLocallySelf)
        {
            _rT.RequestOwnership();
        }
    }
}
