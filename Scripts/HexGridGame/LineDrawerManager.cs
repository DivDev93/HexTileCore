using UnityEngine;

using UnityEngine;
using System.Collections.Generic;
using UnityUtils;

public class LineDrawerManager : Singleton<LineDrawerManager>
{
    [SerializeField] private Material lineMaterial;
    // A simple unlit material that uses something like 
    //   Shader "Unlit/Color" { Properties { _Color("Color", Color) = (1,1,1,1) } ... }
    // or a built-in line shader.

    // Store info about each line
    private class LineSegment
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float expireTime; // if you want a duration
    }

    private static List<LineSegment> _lines = new List<LineSegment>();

    private void Awake()
    {
        lineMaterial = Resources.Load<Material>("LineMaterial");       
    }

    /// <summary>
    /// One-line call to queue a line for rendering this frame (or for duration if specified).
    /// </summary>
    public void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
    {
        if (instance == null)
        {
            Debug.LogWarning("No LineDrawerManager in scene! Create one, or lines won't draw.");
            return;
        }

        var line = VolumetricLinePool.lineStripPool.Get();
        Vector3[] linePositions = { start, end };
        line.UpdateLineVertices(linePositions);
        line.LineColor = color;

        //LineSegment seg = new LineSegment
        //{
        //    start = start,
        //    end = end,
        //    color = color,
        //    expireTime = (duration > 0f) ? Time.time + duration : 0f
        //};
        //_lines.Add(seg);
    }

    //private void Update()
    //{
    //    // If a line has a set duration, remove it once expired
    //    _lines.RemoveAll(line =>
    //        (line.expireTime > 0 && Time.time > line.expireTime));
    //}

    //void OnPostRender()
    //{
    //    if (lineMaterial == null) return;

    //    lineMaterial.SetPass(0);
    //    GL.PushMatrix();
    //    // If you need the lines in world space,
    //    // you might want to do: GL.MultMatrix(transform.localToWorldMatrix);
    //    // or just leave it as identity for direct world coordinates.

    //    GL.Begin(GL.LINES);
    //    foreach (var line in _lines)
    //    {
    //        GL.Color(line.color);
    //        GL.Vertex3(line.start.x, line.start.y, line.start.z);
    //        GL.Vertex3(line.end.x, line.end.y, line.end.z);
    //    }
    //    GL.End();

    //    GL.PopMatrix();
    //}
}
