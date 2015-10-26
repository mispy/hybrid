using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathTarget : PoolBehaviour {
    LineRenderer lineRenderer;
    public Blockform form;
    public List<Vector2> path = new List<Vector2>();
   
    public override void OnCreate() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingLayerName = "UI";
        lineRenderer.gameObject.SetActive(true);
    }

    public void Add(Vector2 point) {
        path.Add(point);

        lineRenderer.SetWidth(0.5f, 0.5f);
        lineRenderer.SetVertexCount(path.Count);
        for (int i = 0; i < path.Count; i++) {
            lineRenderer.SetPosition(i, path[i]);
        }
        
        lineRenderer.SetColors(Color.cyan, Color.cyan);
    }
}

