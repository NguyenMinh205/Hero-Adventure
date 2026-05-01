using System.Collections.Generic;
using UnityEngine;

public class LineRendererManager : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    public Color LineColor
    {
        get => lineRenderer.startColor;
        set
        {
            lineRenderer.startColor = value;
            lineRenderer.endColor = value;
        }
    }
    private List<Vector3> linePoints = new List<Vector3>();

    private void Awake()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    public void AddPoint(Vector3 point)
    {
        point.z = 0;
        linePoints.Add(point);
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPosition(linePoints.Count - 1, point);
    }

    public void RemoveLastPoint()
    {
        if (linePoints.Count > 0)
        {
            linePoints.RemoveAt(linePoints.Count - 1);
            lineRenderer.positionCount = linePoints.Count;

            if (linePoints.Count > 0)
            {
                lineRenderer.SetPosition(linePoints.Count - 1, linePoints[linePoints.Count - 1]);
            }
        }
    }

    public void ClearLine()
    {
        linePoints.Clear();
        lineRenderer.positionCount = 0;
    }
}