using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SyncRigid : PoolBehaviour {
    [HideInInspector]
    public Rigidbody rigid;
    public bool syncFromClient = false;

    void Awake() {
        channel = Channel.Unreliable;
        syncRate = 0.1f;
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnSerialize(ExtendedBinaryWriter writer, bool initial) {
        if (rigid == null) return;

        writer.Write(Network.time);
        writer.Write(rigid.velocity);
        writer.Write(rigid.position);
        writer.Write(rigid.angularVelocity);
        writer.Write(rigid.rotation.eulerAngles);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        if (rigid == null) return;

        var time = reader.ReadDouble();
        var delay = (float)(Network.time - time);

        rigid.velocity = reader.ReadVector3();
        var pos = reader.ReadVector3();
        rigid.position = pos;
        rigid.angularVelocity = reader.ReadVector3();
        var rot = Quaternion.Euler(reader.ReadVector3());
        if (Vector2.Distance(rot.eulerAngles, rigid.rotation.eulerAngles) > 1)
            rigid.rotation = rot;
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

    Vector3 velocity = Vector3.zero;
    Vector3 angularVelocity = Vector3.zero;

	void Update () {
        if (Game.localPlayer.gameObject != this.gameObject && GetComponent<NetworkIdentity>() != null)
            return;
        else if (!SpaceNetwork.isServer)
            return;

        if (Vector3.Distance(rigid.velocity, velocity) > 0.2f || Vector3.Distance(rigid.angularVelocity, angularVelocity) > 0.2f) {
            velocity = rigid.velocity;
            angularVelocity = rigid.angularVelocity;
            SpaceNetwork.Sync(this);
        }

    }
}
