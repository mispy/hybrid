using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SyncRigid : PoolBehaviour {
    Rigidbody rigid;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnSerialize(ExtendedBinaryWriter writer, bool initial) {
        if (rigid == null) return;

        writer.Write(rigid.position);
        writer.Write(rigid.velocity);
        writer.Write(rigid.angularVelocity);
        writer.Write(rigid.rotation.eulerAngles);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        if (rigid == null) return;

        rigid.MovePosition(reader.ReadVector3());
        rigid.velocity = reader.ReadVector3();
        rigid.angularVelocity = reader.ReadVector3();
        rigid.MoveRotation(Quaternion.Euler(reader.ReadVector3()));
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
        SpaceNetwork.Sync(this);
	}
}
