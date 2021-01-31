using System;
using Normal.Realtime;
using UnityEngine;

public class WallLocalMarker : MonoBehaviour
{
    public GameObject wall;
    public float openY;
    public float openSpeed, closeSpeed;
    private GameObject networkedWall;
    bool isNetworkInstance;
    private RealtimeView rtView, childRtView;
    private RealtimeTransform rtTransform, childRtTransform;
    private Realtime _realtime;
    private bool openWall;

    private void OnDrawGizmos()
    {
        if (wall == null) return;
        Gizmos.DrawMesh(wall.GetComponent<MeshFilter>().sharedMesh, transform.position, transform.rotation,
            wall.transform.localScale);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + openY, transform.position.z),
            1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    private void Update()
    {
        if (openWall)
        {
            OpenWallInternal();
        }
        else
        {
            CloseWallInternal();
        }
    }

    private void OpenWallInternal()
    {
        if (isNetworkInstance) return;

        networkedWall.transform.position =
            Vector3.Lerp(networkedWall.transform.localPosition,
                new Vector3(networkedWall.transform.localPosition.x, openY, networkedWall.transform.localPosition.z),
                openSpeed * Time.deltaTime);
    }

    private void CloseWallInternal()
    {
        if (isNetworkInstance) return;
        networkedWall.transform.position =
            Vector3.Lerp(networkedWall.transform.localPosition, Vector3.zero, closeSpeed * Time.deltaTime);
    }

    public void OpenWall()
    {
        openWall = true;
    }

    public void CloseWall()
    {
        openWall = false;
    }

    public void ResetWall()
    {
        if (_realtime == null) _realtime = FindObjectOfType<Realtime>();
        if (rtView == null) rtView = GetComponent<RealtimeView>();
        if (rtTransform == null) rtTransform = GetComponent<RealtimeTransform>();
        if (rtView.isUnownedInHierarchy) rtView.SetOwnership(_realtime.clientID);
        if (rtTransform.isUnownedInHierarchy) rtTransform.SetOwnership(_realtime.clientID);
        isNetworkInstance = rtView.isOwnedLocallyInHierarchy;
        if (isNetworkInstance) return;

        if (networkedWall != null)
        {
            if (!networkedWall.GetComponent<RealtimeView>().isOwnedLocallyInHierarchy)
            {
                networkedWall.GetComponent<RealtimeView>().RequestOwnership();
                networkedWall.GetComponent<RealtimeTransform>().RequestOwnership();
                Realtime.Destroy(networkedWall);
                networkedWall = null; // just to make sure
            }
        }

        networkedWall = Realtime.Instantiate(wall.transform.name,
            position: transform.position,
            rotation: transform.rotation,
            ownedByClient: false,
            preventOwnershipTakeover: false,
            destroyWhenOwnerOrLastClientLeaves: true,
            useInstance: _realtime);

        childRtView = networkedWall.GetComponent<RealtimeView>();
        childRtTransform = networkedWall.GetComponent<RealtimeTransform>();

        if (childRtView.isUnownedInHierarchy) childRtView.SetOwnership(_realtime.clientID);
        if (childRtTransform.isUnownedInHierarchy) childRtTransform.SetOwnership(_realtime.clientID);
    }
}