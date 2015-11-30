using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class Msg {
    public const short Spawn = 1001;
    public const short Sync = 1002;
}

public class SyncMessage : MessageBase {
    public GUID guid;
    public byte[] bytes;

    public SyncMessage() { }

    public SyncMessage(GUID guid, byte[] bytes) {
        this.guid = guid;
        this.bytes = bytes;
    }
}

public class SpawnMessage : MessageBase {
    public string prefabId;
    public byte[] bytes;
    
    public SpawnMessage() { }
    
    public SpawnMessage(string prefabId, byte[] bytes) {
        this.prefabId = prefabId;
        this.bytes = bytes;
    }
}

public struct GUID {
    public string value;

    public override string ToString() {
        return value;
    }
    
    public GUID(string value) {
        this.value = value;
    }
}

public class SpaceNetwork : NetworkManager {   
    static SpawnMessage MakeSpawnMessage(GameObject obj) {
        MemoryStream stream = new MemoryStream();
        var writer = new ExtendedBinaryWriter(stream);
        
        var nets = obj.GetComponents<PoolBehaviour>();
        foreach (var net in nets) {
            if (net.guid.value == null)
                net.guid = new GUID(net.GetInstanceID().ToString());
            writer.Write(net.guid.value);
            net.OnSerialize(writer, true);
        }
        
        var msg = new SpawnMessage(Pool.GetPrefab(obj).name, stream.GetBuffer());
        Debug.LogFormat("[O] SpawnMessage {0} bytes for {1}", msg.bytes.Length, obj.name);        
        return msg;
    }

    static List<GameObject> spawnables = new List<GameObject>();

    public static void Spawn(GameObject obj) {
        if (!NetworkServer.active) return;

        spawnables.Add(obj);
        NetworkServer.SendToAll(Msg.Spawn, MakeSpawnMessage(obj));
    }

    public void OnClientSpawnMessage(NetworkMessage netMsg) {
        if (NetworkServer.active) return;

        SpawnMessage msg = netMsg.ReadMessage<SpawnMessage>();
        var stream = new MemoryStream(msg.bytes);
        var reader = new ExtendedBinaryReader(stream);


        Debug.LogFormat("[I] SpawnMessage {0} bytes for prefab {1}", msg.bytes.Length, msg.prefabId);        

        var obj = Pool.For(msg.prefabId).Attach<Transform>(Game.activeSector.contents);
        foreach (var net in obj.GetComponents<PoolBehaviour>()) {
            net.guid = reader.ReadGUID();
            nets[net.guid] = net;
            net.OnDeserialize(reader, true);
        }
    }

    public static void SyncImmediate(PoolBehaviour net) {
        if (!NetworkServer.active) return;

        if (net.guid.value == null)
            throw new ArgumentException(String.Format("Cannot sync {0} of {1} because it lacks a guid", net.GetType().Name, net.gameObject.name));        
        
        MemoryStream stream = new MemoryStream();
        var writer = new ExtendedBinaryWriter(stream);
        net.OnSerialize(writer, false);
        
        var msg = new SyncMessage(net.guid, stream.GetBuffer());
        
        //Debug.LogFormat("[O] SyncMessage {0} bytes for {1}", msg.bytes.Length, net.guid);        
        NetworkServer.SendToAll(Msg.Sync, msg);

        net.needsSync = false;
        net.syncCountdown = 0.1f;
    }

    public static void Sync(PoolBehaviour net) {
        if (!NetworkServer.active) return;

        net.needsSync = true;
    }

    public void OnClientSyncMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<SyncMessage>();
        var stream = new MemoryStream(msg.bytes);
        var reader = new ExtendedBinaryReader(stream);

        //Debug.LogFormat("[I] SyncMessage {0} bytes for {1}", msg.bytes.Length, msg.guid);

        if (!nets.ContainsKey(msg.guid))
            Debug.LogErrorFormat("Received message for unknown network object {0}", msg.guid);
        else
            nets[msg.guid].OnDeserialize(reader, false);
    }

    public static void Register(GameObject obj) {
        foreach (var net in obj.GetComponents<PoolBehaviour>()) {
            Register(net);
        }
    }

    public static void Register(PoolBehaviour net) {
        if (net.guid.value == null)
            throw new ArgumentException(String.Format("Cannot register {0} of {1} because it lacks a guid", net.GetType().Name, net.gameObject.name));        

        nets[net.guid] = net;
    }

    public static SensibleDictionary<GUID, PoolBehaviour> nets = new SensibleDictionary<GUID, PoolBehaviour>();

    bool needsStart = false;

    NetworkConnection newConn;

    public void Start() {
        NetworkServer.RegisterHandler(Msg.Sync, OnClientSyncMessage);
        NetworkServer.RegisterHandler(Msg.Spawn, OnClientSpawnMessage);
    }

    public void Update() {
        if (needsStart) {
            Game.Start();
            needsStart = false;
        }

        foreach (var net in nets.Values) {
            if (net.needsSync && net.syncCountdown <= 0f) {
                SyncImmediate(net);
            } else if (net.syncCountdown > 0f) {
                net.syncCountdown -= Time.deltaTime;
            }
        }
    }

    public override void OnStartServer() {
        needsStart = true;
    }

    public override void OnServerConnect(NetworkConnection conn) {
        foreach (var spawnable in spawnables) {
            var msg = MakeSpawnMessage(spawnable);
            conn.Send(Msg.Spawn, msg);
        }
    }

    public override void OnStartClient(NetworkClient client) {        
        BlockType.LoadTypes();
        Star.Setup();
        Game.state.gameObject.SetActive(true);
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        conn.RegisterHandler(Msg.Sync, OnClientSyncMessage);
        conn.RegisterHandler(Msg.Spawn, OnClientSpawnMessage);
    }
}

