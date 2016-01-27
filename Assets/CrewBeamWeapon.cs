using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CrewBeamWeapon : MonoBehaviour {
    CrewBody crew;
    LineRenderer line;
    float range = 5f;
    float beamDuration = 0.1f;
    float betweenShotsInterval = 0.05f;

    float beamCounter = 0f;
    float betweenShotsCounter = 0f;
    float damageAmount = 10f;

    void Awake() {
        crew = GetComponentInParent<CrewBody>();
        line = Pool.For("Line").Attach<LineRenderer>(transform);
        line.sortingLayerName = "UI";
    }

    public void Fire(Vector2 targetPos) {
        if (beamCounter > 0 || betweenShotsCounter > 0) return;

        var fireDir = (targetPos - (Vector2)transform.position).normalized;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, fireDir, range, LayerMask.GetMask(new string[] { "Crew", "Wall" }));


        var beamEndPos = (Vector2)transform.position + fireDir*range;
        foreach (var hit in hits.OrderBy((hit) => Vector2.Distance(transform.position, hit.point))) {
            if (hit.collider != crew.collider) {
                beamEndPos = hit.point;
                var otherCrew = hit.collider.GetComponent<CrewBody>();
                if (otherCrew != null) otherCrew.TakeDamage(damageAmount);
                break;
            }
        }

        beamCounter = beamDuration;
        betweenShotsCounter = betweenShotsInterval;
        line.enabled = true;
        line.SetWidth(0.2f, 0.2f);
        line.SetVertexCount(2);
        line.SetPosition(0, Vector2.zero);
        line.SetPosition(1, transform.InverseTransformPoint(beamEndPos));
        line.SetColors(Color.yellow, Color.yellow);
    }
	
	// Update is called once per frame
	void Update () {
        if (beamCounter > 0f) {
            beamCounter -= Time.deltaTime;
            if (beamCounter <= 0f) {
                line.enabled = false;
            }
        } else if (betweenShotsCounter > 0f) {
            betweenShotsCounter -= Time.deltaTime;
        }
	}
}
