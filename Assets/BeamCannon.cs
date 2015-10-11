using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BeamCannon : BlockComponent {
    LineRenderer lineRenderer;
    Blockform targetForm;
    List<Vector2> beamPath;
    float beamDuration = 2f;
    float beamElapsed = 0f;
    bool isFiring = false;

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    public void OnPathTarget(PathTarget target) {
        Debug.Log("OnPathTarget");
        Fire(target.form, target.path);
    }

    public void Fire(Blockform targetForm, List<Vector2> points) {        
        targetForm = targetForm;
        beamPath = points;
        beamElapsed = 0f;
        isFiring = true;
        lineRenderer.enabled = true;
    }

    void Update() {
        if (!isFiring) return;
        var currentBeamPos = Util.PathLerp(beamPath, beamElapsed/beamDuration);

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, Util.TipPosition(block));
        lineRenderer.SetPosition(1, currentBeamPos);
        lineRenderer.SetColors(Color.yellow, Color.yellow);

        beamElapsed += Time.deltaTime;
        if (beamElapsed > beamDuration) {
            isFiring = false;
            lineRenderer.enabled = false;
        }        
    }
}
