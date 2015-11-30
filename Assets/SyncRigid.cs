using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SyncRigid : NetworkBehaviour {
    [SyncVar]
    Vector2 velocity;
    [SyncVar]
    Vector3 angularVelocity;
    [SyncVar]
    Quaternion rotation;

    Rigidbody rigid;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
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

	void Update () {
	    velocity = rigid.velocity;
        angularVelocity = rigid.angularVelocity;
        rotation = rigid.rotation;
	}
}
