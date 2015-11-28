using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class Thruster : BlockComponent {
    [HideInInspector]
    public ParticleSystem ps;

    public bool isFiring = false;
    public bool isFiringAttitude = false;

    void Start() {
        ps = GetComponentInChildren<ParticleSystem>();
    }
    
    public void Fire() {        
        if (!block.isPowered)
            return;

        isFiringAttitude = false;
        isFiring = true;
        CancelInvoke("Stop");
        Invoke("Stop", 0.1f);

        NetworkSync();
    }

    public void FireAttitude() {
        if (!block.isPowered)
            return;

        isFiring = false;
        isFiringAttitude = true;
        CancelInvoke("Stop");
        Invoke("Stop", 0.1f);

        NetworkSync();
    }

    public void Stop() {
        isFiring = false;
        isFiringAttitude = false;

        NetworkSync();
    }

    public override void OnSerialize(ExtendedBinaryWriter writer) {
        writer.Write(isFiring);
        writer.Write(isFiringAttitude);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader) {
        isFiring = reader.ReadBoolean();
        isFiringAttitude = reader.ReadBoolean();
    }

    void Update() {
        if (isFiring || isFiringAttitude) {
            ps.Emit(1);
        }
    }

    void FixedUpdate() {
        if (isFiring) {
            form.rigidBody.AddForce(-transform.up * Time.fixedDeltaTime * 100f);
        } else if (isFiringAttitude) {            
            var dist = transform.localPosition - form.centerOfMass;
            var force = Time.fixedDeltaTime*200f;

            if (dist.x > 0) {
                form.rigidBody.AddRelativeTorque(Vector3.forward * force);
            } else {
                form.rigidBody.AddRelativeTorque(Vector3.back * force);
            }
        }
    }
}
