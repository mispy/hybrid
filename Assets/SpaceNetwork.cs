using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class Msg {
    public const short Spawn = 1001;
    public const short Sync = 1002;
    public const short Call = 1003;
}

public static class Channel {
    public const short ReliableSequenced = 0;
    public const short ReliableFragmented = 1;
    public const short Unreliable = 2;
    public const short UnreliableSequenced = 3;
}

public class SyncMessage : MessageBase {
    public int timestamp;
    public GUID guid;
    public byte[] bytes;

    public SyncMessage() { }

    public SyncMessage(GUID guid, byte[] bytes) {
        this.timestamp = NetworkTransport.GetNetworkTimestamp();
        this.guid = guid;
        this.bytes = bytes;
    }
}

public class SpawnMessage : MessageBase {
    public string prefabId;
    public Vector2 position;
    public Quaternion rotation;
    public byte[] bytes;
    
    public SpawnMessage() { }
    
    public SpawnMessage(string prefabId, Vector2 position, Quaternion rotation, byte[] bytes) {
        this.prefabId = prefabId;
        this.position = position;
        this.rotation = rotation;
        this.bytes = bytes;
    }
}

public class CallMessage : MessageBase {
    public GUID guid;
    public string methodName;

    public CallMessage() { }
    public CallMessage(GUID guid, string methodName) {
        this.guid = guid;
        this.methodName = methodName;
    }
}

public struct GUID {
    static int counter = 0;

    public static GUID Assign() {
        return new GUID(counter += 1);
    }

    public static void Reserve(int amount) {
        counter += amount;
    }

    public static bool operator ==(GUID v1, GUID v2) {
        return v1.isValid && v2.isValid && v1.value == v2.value;
    }

    public static bool operator !=(GUID v1, GUID v2) {
        return !v1.isValid || !v2.isValid || v1.value != v2.value;
    }

    public int value;
        
    public bool isValid {
        get { return value != 0; }
    }

    public GUID(int value) {
        this.value = value;
    }

    public override int GetHashCode()
    {
        return value;
    }

    public override string ToString() {
        return value.ToString();
    }
}

public class SpaceNetwork : NetworkManager {   
    public static bool isServer {
        get {
            return NetworkServer.active;
        }
    }

    public static bool isClient {
        get {
            return NetworkClient.active;
        }
    }

    public static Dictionary<PoolBehaviour, long> stats = new Dictionary<PoolBehaviour, long>();

    static void AddStat(PoolBehaviour net, long bytes) {
        if (!stats.ContainsKey(net))
            stats[net] = 0;

        stats[net] += bytes;
    }

    static SpawnMessage MakeSpawnMessage(GameObject obj) {
        MemoryStream stream = new MemoryStream();
        var writer = new ExtendedBinaryWriter(stream);

        var length = stream.Length;
        var nets = obj.GetComponents<PoolBehaviour>();
        foreach (var net in nets) {
            if (!net.guid.isValid)
                net.guid = GUID.Assign();
            writer.Write(net.guid.value);
            net.OnSerialize(writer, true);

            var added = stream.Length - length;
            AddStat(net, added);
            length = stream.Length;
        }
        
        var msg = new SpawnMessage(Pool.GetPrefab(obj).name, obj.transform.position, obj.transform.rotation, stream.GetBuffer());
        //Debug.LogFormat("[O] SpawnMessage {0} bytes for {1}", msg.bytes.Length, obj.name);        
        return msg;
    }

    public static List<GameObject> spawnables = new List<GameObject>();

    public static void Spawn(GameObject obj) {
        if (!isServer) return;

        spawnables.Add(obj);

        var msg = MakeSpawnMessage(obj);
        if (msg.bytes.Length >= 1500) {
            NetworkServer.SendByChannelToAll(Msg.Spawn, msg, Channel.ReliableFragmented);
        } else {
            NetworkServer.SendByChannelToAll(Msg.Spawn, msg, Channel.ReliableSequenced);
        }

        if (isClient) {
            UnpackSpawnMessage(obj, msg);
        }

        Register(obj);
    }

    static void UnpackSpawnMessage(GameObject obj, SpawnMessage msg) {               
        obj.transform.position = msg.position;
        obj.transform.rotation = msg.rotation;

        var stream = new MemoryStream(msg.bytes);
        var reader = new ExtendedBinaryReader(stream);

        foreach (var net in obj.GetComponents<PoolBehaviour>()) {
            net.guid = reader.ReadGUID();

            nets[net.guid] = net;

            try {
                net.deserializing = true;
                net.OnDeserialize(reader, true);
            } finally {
                net.deserializing = false;
            }
        }
    }

    public void OnClientSpawnMessage(NetworkMessage netMsg) {
        if (isServer) return;

        SpawnMessage msg = netMsg.ReadMessage<SpawnMessage>();

        //Debug.LogFormat("[I] SpawnMessage {0} bytes for prefab {1}", msg.bytes.Length, msg.prefabId);        

        var obj = Pool.For(msg.prefabId).Attach<Transform>(Game.activeSector.contents).gameObject;
        UnpackSpawnMessage(obj, msg);
    }

    public static void SyncImmediate(PoolBehaviour net) {
        if (!net.guid.isValid)
            throw new ArgumentException(String.Format("Cannot sync {0} of {1} because it lacks a guid", net.GetType().Name, net.gameObject.name));        
        
        MemoryStream stream = new MemoryStream();
        var writer = new ExtendedBinaryWriter(stream);
        net.OnSerialize(writer, false);

        if (stream.Length == 0) return;

        AddStat(net, stream.Length);
        
        var msg = new SyncMessage(net.guid, stream.GetBuffer());

        //Debug.LogFormat("[O] SyncMessage {0} bytes for {1} {2} ({3})", msg.bytes.Length, net.gameObject.name, net.GetType().Name, net.guid);        
        if (isServer) {
            NetworkServer.SendByChannelToAll(Msg.Sync, msg, net.channel);
        } else {
            NetworkClient.allClients[0].SendByChannel(Msg.Sync, msg, net.channel);       
        }

        net.needsSync = false;
        net.syncCountdown = net.syncRate;
    }

    public static void Sync(PoolBehaviour net) {
        net.needsSync = true;
        if (net.syncCountdown <= 0f)
            SyncImmediate(net);
    }

    public void OnSyncMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<SyncMessage>();
        var stream = new MemoryStream(msg.bytes);
        var reader = new ExtendedBinaryReader(stream);

        if (!nets.ContainsKey(msg.guid)) {
            //Debug.LogWarningFormat("Received message for unknown network object {0}", msg.guid);
        } else {
            var net = nets[msg.guid];
            net.lastSyncMessage = msg;
            byte err;
            if (netMsg.conn.hostId != -1)
                net.syncDeltaTime = NetworkTransport.GetRemoteDelayTimeMS(netMsg.conn.hostId, netMsg.conn.connectionId, msg.timestamp, out err)/1000f;
            else
                net.syncDeltaTime = 0f;
            //Debug.LogFormat("{0} {1} {2}", net.lastSyncMessage.timestamp, net.lastSyncDelay, NetworkTransport.GetNetworkTimestamp());
            net.lastSyncReceived = Time.time;

            try {
                net.deserializing = true;
                net.OnDeserialize(reader, false);
            } finally {
                net.deserializing = false;
            }
        }

        //Debug.LogFormat("[I] SyncMessage {0} bytes for {1} {2} ({3})", msg.bytes.Length, msg.guid, nets[msg.guid].gameObject.name, nets[msg.guid].GetType().Name);

        // If we're the server, redistribute this message to everyone else
        if (isServer) {
            foreach (var conn in NetworkServer.connections) {
                if (conn != null && conn != netMsg.conn && conn.connectionId != 0)
                    conn.SendByChannel(Msg.Sync, msg, netMsg.channelId);
            }
        }
    }

    public static void OnCallMessage(NetworkMessage netMsg) {
        var msg = netMsg.ReadMessage<CallMessage>();

        if (!nets.ContainsKey(msg.guid))
            Debug.LogErrorFormat("Received call message for unknown network object {0} method {1}", msg.guid, msg.methodName);
        else
            nets[msg.guid].SendMessage(msg.methodName);
    }

    public static void Register(GameObject obj) {
        foreach (var net in obj.GetComponents<PoolBehaviour>()) {
            Register(net);
        }
    }

    public static void Register(PoolBehaviour net) {
        if (!net.guid.isValid)
            throw new ArgumentException(String.Format("Cannot register {0} of {1} because it lacks a guid", net.GetType().Name, net.gameObject.name));        

        nets[net.guid] = net;
    }
        
    public static void ServerCall(PoolBehaviour net, string methodName) {
        if (isServer)
            net.SendMessage(methodName);
        else {
            var msg = new CallMessage(net.guid, methodName);
            NetworkClient.allClients[0].SendByChannel(Msg.Call, msg, net.channel);
        }
    }

    public static SensibleDictionary<GUID, PoolBehaviour> nets = new SensibleDictionary<GUID, PoolBehaviour>();

    bool needsStart = false;

    NetworkConnection newConn;


    public void Start() {
        NetworkServer.RegisterHandler(Msg.Sync, OnSyncMessage);
        NetworkServer.RegisterHandler(Msg.Spawn, OnClientSpawnMessage);
        NetworkServer.RegisterHandler(Msg.Call, OnCallMessage);
    }


    public void ServerUpdate() {
    }

    public void ClientUpdate() {
    }

    public void Update() {
        if (needsStart) {
            Game.Start();
            needsStart = false;
        }

        if (isServer)
            ServerUpdate();
        else
            ClientUpdate();

        foreach (var guid in nets.Keys.ToList()) {
            var net = nets[guid];
            if (net == null) {
                nets.Remove(guid);
                continue;
            }
            
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

    static bool seenServer = false;

    Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    public override void OnServerConnect(NetworkConnection conn) {
        if (conn.connectionId == 0 && !seenServer) {
            seenServer = true;
            return;
        }

        foreach (var spawnable in spawnables) {
            if (spawnable == null) continue;
            var msg = MakeSpawnMessage(spawnable);
            conn.SendByChannel(Msg.Spawn, msg, Channel.ReliableFragmented);
        }

        var crew = Pool.For("Crew").Attach<CrewBody>(Game.activeSector.contents);
        crew.connectionId = conn.connectionId;
        SpaceNetwork.Spawn(crew.gameObject);

        players[conn.connectionId] = crew.gameObject;
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        Pool.Recycle(players[conn.connectionId].gameObject);
        players.Remove(conn.connectionId);
    }

    public override void OnStartClient(NetworkClient client) {        
        BlockType.LoadTypes();
        Star.Setup();
        Game.state.gameObject.SetActive(true);  
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        conn.RegisterHandler(Msg.Sync, OnSyncMessage);
        conn.RegisterHandler(Msg.Spawn, OnClientSpawnMessage);
    }

    public void OnDrawGizmos() {
        foreach (var net in nets.Values) {
            if (net == null) continue;

            if (net.syncCountdown > 0f) {
                Gizmos.color = SpaceColor.friendly;
                Gizmos.DrawSphere(net.transform.position, 0.5f);
            }                       

            if (net.lastSyncReceived > Time.time - 0.1f) {
                Gizmos.color = SpaceColor.neutral;
                Gizmos.DrawSphere(net.transform.position, 0.3f);
            }

        }
    }
}

