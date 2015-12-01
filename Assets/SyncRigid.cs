using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SyncRigid : PoolBehaviour {
    [HideInInspector]
    public Rigidbody rigid;
    public bool syncFromClient = false;
    public float lastSyncTime = 0f;

    void Awake() {
        channel = Channel.UnreliableSequenced;
        syncRate = 0.1f;
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnSerialize(ExtendedBinaryWriter writer, bool initial) {
        if (rigid == null) return;

        writer.Write((Vector2)rigid.position);
        writer.Write(rigid.rotation.eulerAngles.z);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        if (rigid == null) return;

        var pos = reader.ReadVector2();
        var rot = Quaternion.Euler(new Vector3(0, 0, reader.ReadSingle()));
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

        if (Vector3.Distance(rigid.velocity, velocity) > 0.2f || Vector3.Distance(rigid.angularVelocity, angularVelocity) > 0.2f || GetComponent<Blockform>() != null) {
            velocity = rigid.velocity;
            angularVelocity = rigid.angularVelocity;
            SpaceNetwork.Sync(this);
        }

    }
}
