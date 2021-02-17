using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class InformationLine : MonoBehaviour
{
    public Transform target;
    [Range(0, 10)] public float lineThickness;

    public Camera UICamera;

    VectorLine myLine;

    void Start()
    {
        var points = new List<Vector2>()
            {UICamera.WorldToScreenPoint(transform.position), UICamera.WorldToScreenPoint(target.position)};
        myLine = new VectorLine("Line", points, lineThickness);
        points = new List<Vector2>() {UICamera.WorldToScreenPoint(target.position)};
        myLine.color = Color.yellow;
        myLine.Draw();
        VectorLine.SetCanvasCamera(UICamera);
        VectorLine.canvas.planeDistance = 1f;
    }

    void Update()
    {
        myLine.points2[0] = UICamera.WorldToScreenPoint(transform.position);
        myLine.points2[1] = UICamera.WorldToScreenPoint(target.position);
        myLine.SetWidth(lineThickness);
        myLine.Draw();
    }
}