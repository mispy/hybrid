using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BeamCannon : BlockComponent {
    public float damage;
    public float hitRadius;

    CooldownCharger charger;
    LineRenderer lineRenderer;
    float beamDuration = 2f;
    float beamElapsed = 0f;
    bool isFiring = false;

    public override void OnCreate() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.sortingLayerName = "UI";
        charger = GetComponent<CooldownCharger>();
    }

    public void OnFire() {
        //Debug.Log(Util.TurretBlocked(form, Util.TipPosition(block), Game.mousePos));
        if (charger.isReady) //&& !Util.TurretBlocked(form, Util.TipPosition(block), Game.mousePos)) {
            Fire();
    }

    public void Fire() {        
        charger.Discharge();
        beamElapsed = 0f;
        isFiring = true;
        charger.isPaused = true;
        lineRenderer.enabled = true;
    }

    void UpdateFiring() {
        var currentBeamPos = Game.mousePos;//Util.PathLerp(target.path, beamElapsed/beamDuration);
        
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, Util.TipPosition(block));
        lineRenderer.SetPosition(1, currentBeamPos);
        lineRenderer.SetColors(Color.yellow, Color.yellow);

        var targetForm = Blockform.AtWorldPos(Game.mousePos);

        if (targetForm != null) {
            foreach (var targetBlock in targetForm.BlocksInWorldRadius(currentBeamPos, hitRadius)) {
                targetForm.damage.DamageBlock(targetBlock, damage * (Time.deltaTime/beamDuration));
            }
        }

        beamElapsed += Time.deltaTime;
        if (!Input.GetMouseButton(0) || beamElapsed > beamDuration) {
            isFiring = false;
            charger.isPaused = false;
            lineRenderer.enabled = false;
        }        
    }

    void Update() {
        if (isFiring)
            UpdateFiring();
    }
}
