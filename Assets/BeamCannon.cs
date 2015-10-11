using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BeamCannon : BlockComponent {
    CooldownCharger charger;
    LineRenderer lineRenderer;
    PathTarget target;
    float beamDuration = 2f;
    float beamElapsed = 0f;
    bool isFiring = false;

    public override void OnCreate() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.sortingLayerName = "UI";
        charger = GetComponent<CooldownCharger>();
    }

    public void OnPathTarget(PathTarget target) {
        this.target = target;
    }

    public void Fire() {        
        charger.Discharge();
        beamElapsed = 0f;
        isFiring = true;
        charger.isPaused = true;
        lineRenderer.enabled = true;
    }

    void UpdateFiring() {
        var currentBeamPos = Util.PathLerp(target.path, beamElapsed/beamDuration);
        
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, Util.TipPosition(block));
        lineRenderer.SetPosition(1, target.transform.TransformPoint(currentBeamPos));
        lineRenderer.SetColors(Color.yellow, Color.yellow);
        
        beamElapsed += Time.deltaTime;
        if (beamElapsed > beamDuration) {
            isFiring = false;
            charger.isPaused = false;
            lineRenderer.enabled = false;
        }        
    }

    void UpdateWaiting() {
        if (charger.isReady && target != null) {
            Fire();
        }
    }

    void Update() {
        if (isFiring)
            UpdateFiring();
        else
            UpdateWaiting();

    }
}
