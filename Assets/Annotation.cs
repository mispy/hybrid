using UnityEngine;
using System.Collections;

public class Annotation : MonoBehaviour {
    static Annotation anno;
    static LineRenderer lineRenderer;

    public static LineRenderer DrawLine(Vector2 start, Vector2 end, Color color, float width) {
        var line = Pool.For("Line").Attach<LineRenderer>(Annotation.anno.transform);
        line.SetWidth(width, width);
        line.SetVertexCount(2);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.SetColors(color, color);
        return line;
    }

    void Awake() {
        Annotation.anno = this;
        Annotation.lineRenderer = GetComponent<LineRenderer>();
    }
}
