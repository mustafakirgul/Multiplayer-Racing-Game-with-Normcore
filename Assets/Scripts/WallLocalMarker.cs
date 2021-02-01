using System;
using Normal.Realtime;
using UnityEngine;

public class WallLocalMarker : MonoBehaviour
{
    public GameObject wall;
    public float openY;
    [SerializeField] private float targetY, currentY;
    public float speed;
    private GameObject networkedWall;
    bool isNetworkInstance = true;
    private RealtimeView rtView, childRtView;
    private RealtimeTransform rtTransform, childRtTransform;
    private Realtime _realtime;
    [SerializeField] private bool isRunning;
    public bool showPreview;

    private void OnDrawGizmos()
    {
        if (wall == null || !showPreview) return;
        Gizmos.DrawMesh(wall.GetComponent<MeshFilter>().sharedMesh, transform.position, transform.rotation,
            wall.transform.localScale);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(0f, openY, 0f)),
            1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    private void Update()
    {
        if (isNetworkInstance) return;
        if (isRunning && networkedWall != null)
        {
            currentY = Mathf.Lerp(currentY, targetY, speed * Time.deltaTime);
            networkedWall.transform.position = transform.position + new Vector3(0, currentY, 0);
            if (Mathf.Abs(targetY - currentY) < .5f)
            {
                currentY = targetY;
                isRunning = false;
            }
        }
    }

    public void OpenWall()
    {
        targetY = openY;
        isRunning = true;
    }

    public void CloseWall()
    {
        targetY = 0f;
        isRunning = true;
    }

    public void ResetWall()
    {
        if (_realtime == null) _realtime = FindObjectOfType<Realtime>();
        if (rtView == null) rtView = GetComponent<RealtimeView>();
        if (rtTransform == null) rtTransform = GetComponent<RealtimeTransform>();
        if (rtView.isUnownedSelf)
        {
            rtView.SetOwnership(_realtime.clientID);
            rtTransform.SetOwnership(_realtime.clientID);
        }

        isNetworkInstance = !rtView.isOwnedLocallyInHierarchy;
        if (isNetworkInstance) return;

        if (networkedWall != null)
        {
            networkedWall.GetComponent<RealtimeView>().SetOwnership(_realtime.clientID);
            networkedWall.GetComponent<RealtimeTransform>().SetOwnership(_realtime.clientID);
            if (!networkedWall.GetComponent<RealtimeView>().isUnownedInHierarchy)
                Realtime.Destroy(networkedWall);
            Debug.LogWarning("Existing wall removed!");
        }

        networkedWall = Realtime.Instantiate(wall.transform.name,
            position: transform.position,
            rotation: transform.rotation,
            ownedByClient: false,
            preventOwnershipTakeover: false,
            destroyWhenOwnerOrLastClientLeaves: true,
            useInstance: _realtime);
        Debug.LogWarning("Networked Wall Instantiated");
        childRtView = networkedWall.GetComponent<RealtimeView>();
        childRtTransform = networkedWall.GetComponent<RealtimeTransform>();

        if (childRtView.isUnownedSelf)
        {
            childRtView.SetOwnership(_realtime.clientID);
            childRtTransform.SetOwnership(_realtime.clientID);
        }
    }
}