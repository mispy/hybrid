using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Explosive))]
public class ExplosiveCollisionTrigger : MonoBehaviour {
    Explosive explosive;

    void Awake() {
        explosive = GetComponent<Explosive>();
    }        

    void OnCollisionEnter(Collision col) {
        if (!explosive.hasStarted || explosive.shouldExplode || !SpaceNetwork.isServer) return;

        if (enabled && col.contacts.Length > 0) {
            // compare relative velocity to collision normal - so we don't explode from a fast but gentle glancing collision
            //float velocityAlongCollisionNormal =
            //    Vector3.Project(col.relativeVelocity, col.contacts[0].normal).magnitude;

            //if (velocityAlongCollisionNormal > detonationImpactVelocity)
            //{

            explosive.shouldExplode = true;
            SpaceNetwork.Sync(explosive);
            //}
        }
    }
}
