using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        var originPoint = Util.TipPosition(block);

        RaycastHit[] hits = Physics.RaycastAll(Util.TipPosition(block), (Game.mousePos - Util.TipPosition(block)).normalized);

        foreach (var hit in hits.OrderBy((hit) => Vector2.Distance(originPoint, hit.point))) {
            if (hit.collider.attachedRigidbody != block.ship.rigidBody && hit.collider.GetComponent<Shields>() != null) {
                currentBeamPos = hit.point;
                break;
            }
        }

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, Util.TipPosition(block));
        lineRenderer.SetPosition(1, currentBeamPos);
        lineRenderer.SetColors(Color.yellow, Color.yellow);

        var targetForm = Blockform.AtWorldPos(currentBeamPos);

        if (targetForm != null) {
            foreach (var targetBlock in targetForm.BlocksInWorldRadius(currentBeamPos, hitRadius)) {
                targetBlock.health -= damage * (Time.deltaTime/beamDuration);
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
