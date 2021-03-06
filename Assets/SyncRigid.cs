﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SyncRigid : PoolBehaviour {
    [HideInInspector]
    public Rigidbody rigid;

    Vector2 lastPos = Vector2.zero;
    Quaternion lastRot = Quaternion.identity;
    float lastTime = 0f;

    Vector2 nextPos = Vector2.zero;
    Quaternion nextRot = Quaternion.identity;
    float nextTime = 0f;

    bool isReady = false;
    float lerpCounter = 0f;

    void Awake() {
        channel = Channel.Unreliable;
        syncRate = 0.1f;
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnSerialize(MispyNetworkWriter writer, bool initial) {
        if (rigid == null) return;

        writer.Write((Vector2)rigid.position);
        writer.Write(rigid.rotation.eulerAngles.z);
    }

    public override void OnDeserialize(MispyNetworkReader reader, bool initial) {
        if (rigid == null) return;

        if (!initial)
            isReady = true;

        lastPos = rigid.position;
        lastRot = rigid.rotation;
        lastTime = Time.time;

        nextPos = reader.ReadVector2();
        nextRot = Quaternion.Euler(new Vector3(0, 0, reader.ReadSingle()));
        nextTime = Time.time + syncRate;

        lerpCounter = 0f;
    }

    public void OnEnable() {
        isReady = false;
    }

    /*public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        if (!initialState && syncVarDirtyBits == 0) return false;

        writer.Write(velocity);
        writer.Write(angularVelocity);
        writer.Write(rotation);
        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        rigid.velocity = reader.ReadVector2();
        rigid.angularVelocity = reader.ReadVector3();
        rigid.rotation = reader.ReadQuaternion();
    }*/


	void Update() {
        if (!hasAuthority && isReady) {
            lerpCounter += Time.deltaTime;
            var progress = lerpCounter / (nextTime - lastTime);
            //Debug.LogFormat("{0} {1} {2}", lastPos, nextPos, progress);
            rigid.position = Vector2.Lerp(lastPos, nextPos, progress);
            rigid.rotation = Quaternion.Lerp(lastRot, nextRot, progress);
        }

        if (hasAuthority) {
//            if (Vector3.Distance(rigid.velocity, velocity) > 0.2f || Vector3.Distance(rigid.angularVelocity, angularVelocity) > 0.2f || GetComponent<Blockform>() != null) {
                SpaceNetwork.Sync(this);
//            }
        }
    }
}
