using UnityEngine;
using System;
using System.Collections;

public class Thruster : BlockComponent {
    [HideInInspector]
    public ParticleSystem ps;
    [HideInInspector]
    public Blockform form;

    public PowerReceiver power;

    public bool isFiring = false;
    public bool isFiringAttitude = false;

    void Start() {
        form = GetComponentInParent<Blockform>();
        ps = GetComponentInChildren<ParticleSystem>();
        power = GetComponent<PowerReceiver>();
    }
    
    public void Fire() {        
        //if (!power.isPowered)
        //    return;

        isFiringAttitude = false;
        isFiring = true;
        CancelInvoke("Stop");
        Invoke("Stop", 0.1f);
    }

    public void FireAttitude() {
        //if (!power.isPowered)
        //    return;

        isFiring = false;
        isFiringAttitude = true;
        CancelInvoke("Stop");
        Invoke("Stop", 0.1f);
    }

    public void Stop() {
        isFiring = false;
        isFiringAttitude = false;
    }

    void Update() {
        if (isFiring || isFiringAttitude) {
            ps.Emit(1);
        }
    }

    void FixedUpdate() {
        if (isFiring) {
            form.rigidBody.AddForce((-transform.up * form.rigidBody.mass) * Time.fixedDeltaTime * 300f);
        } else if (isFiringAttitude) {            
            var dist = transform.localPosition - form.centerOfMass;
            var force = form.blocks.baseSize * 10 * Time.fixedDeltaTime;
            
            if (dist.x > 0) {
                form.rigidBody.AddRelativeTorque(Vector3.forward * force);
            } else {
                form.rigidBody.AddRelativeTorque(Vector3.back * force);
            }
        }
    }
}
