using UnityEngine;
using System.Collections;

public class SectorBounds : MonoBehaviour {
    public bool IsOutsideBounds(Vector2 pos) {
        return pos.magnitude > transform.localScale.x;
    }

    public void FixedUpdate() {
        foreach (var form in Game.activeSector.blockforms) {            
            if (!IsOutsideBounds(form.transform.position)) continue;

            var towardsCenter = (Vector3.zero - form.transform.position).normalized;
            var factor = form.transform.position.magnitude - transform.localScale.x;
            form.rigidBody.AddForce(towardsCenter * factor * 10 * Time.fixedDeltaTime);
        }
    }
}
