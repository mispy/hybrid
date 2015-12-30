using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CrewBody : PoolBehaviour {
    [HideInInspector]
    public Rigidbody rigidBody;
    [HideInInspector]
    public SyncRigid syncRigid;
    public new BoxCollider collider;
    public Constructor constructor;
    public int connectionId;
    
    // Crew can be maglocked to a ship even if they don't have a current block
    // bc they attach to the sides as well
    public Blockform maglockShip = null;
    public IntVector2 currentBlockPos;
    public IntVector2 maglockMoveBlockPos;
    public Block currentBlock = null;

    public bool isMaglocked {
        get {
            return maglockShip != null;
        }
    }

    public Block controlConsole = null;
    
    public CrewWeapon weapon;
    [ReadOnlyAttribute]
    public CrewMind mind;

    public int maxHealth;
    public int health;

    public override void OnSerialize(ExtendedBinaryWriter writer, bool initial) {
        if (initial) {
            writer.Write(connectionId);
            return;
        }

        writer.Write(maglockShip);    
        writer.Write(maglockMoveBlockPos);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        if (initial) {
            connectionId = reader.ReadInt32();
            if (connectionId == NetworkClient.allClients[0].connection.connectionId)
                gameObject.AddComponent<Player>();            
            return;
        }

        var ship = reader.ReadComponent<Blockform>();
        var movePos = reader.ReadIntVector2();

        if (ship != null && maglockShip != ship) {
            SetMaglock(ship);
        } else if (ship == null && maglockShip != null) {
            StopMaglock();
        }

        maglockMoveBlockPos = movePos;
    }

    public void TakeDamage(int amount) {
        health -= amount;
        
        if (health <= 0)
            Pool.Recycle(this.gameObject);
    }

    void Awake() {
        channel = Channel.ReliableSequenced;
        collider = GetComponent<BoxCollider>();
        weapon = gameObject.AddComponent<CrewWeapon>();
        rigidBody = GetComponent<Rigidbody>();
        syncRigid = GetComponent<SyncRigid>();
        //mind = gameObject.AddComponent<CrewMind>();      
    }

    public void MaglockMove(IntVector2 bp) {
        maglockMoveBlockPos = bp;
        SpaceNetwork.Sync(this);
    }
    
    public void UseBlock(Block block)  {
        controlConsole = block;
    }
    
    public void SetMaglock(Blockform ship) {
        maglockShip = ship;
        maglockShip.maglockedCrew.Add(this);
        transform.rotation = maglockShip.transform.rotation;
        transform.SetParent(maglockShip.transform);
        syncRigid.enabled = false;
        //Destroy(rigidBody);
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = true;
        maglockMoveBlockPos = ship.WorldToBlockPos(transform.position);
        collider.isTrigger = true;

        if (hasAuthority && !deserializing)
            SpaceNetwork.SyncImmediate(this);
        
        //if (this == Crew.player)
        //    maglockShip.blueprint.blocks.EnableRendering();
    }
    
    void StopMaglock() {
        transform.SetParent(Game.activeSector.contents);
        maglockShip.maglockedCrew.Remove(this);
        //AddRigid();
        //if (this == Crew.player)
        //    maglockShip.blueprint.blocks.DisableRendering();
        syncRigid.enabled = true;
        rigidBody.isKinematic = false;
        collider.isTrigger = false;
        maglockShip = null;
        currentBlock = null;

        if (hasAuthority && !deserializing)
            SpaceNetwork.SyncImmediate(this);        
    }
    
    bool CanMaglock(Blockform form) {
        var blockPos = form.WorldToBlockPos(transform.position);
        
        if (form.blocks[blockPos, BlockLayer.Base] != null)
            return true;
        
        foreach (var bp in IntVector2.NeighborsWithDiagonal(blockPos)) {
            if (form.blocks[bp, BlockLayer.Base] != null)
                return true;
        }
        
        return false;
    }
    
    Blockform FindMaglockShip() {
        if (maglockShip != null && CanMaglock(maglockShip)) {
            return maglockShip;
        } else {
            foreach (var form in Game.activeSector.blockforms) {
                if (CanMaglock(form)) return form;
            }
            
            return null;
        }
    }
    
    void UpdateMaglockMove() {
        if (maglockShip.blocks[maglockMoveBlockPos, BlockLayer.Base] != null && !maglockShip.blocks.IsPassable(maglockMoveBlockPos))
			return;

        var speed = 10f;
        var targetPos = maglockShip.BlockToLocalPos(maglockMoveBlockPos);

        var pos = (Vector2)transform.localPosition;
        var dist = targetPos - pos;
        
        if (dist.magnitude > speed*Time.deltaTime) {
            dist.Normalize();
            dist = dist*speed*Time.deltaTime;
        }
        
        transform.localPosition += (Vector3)dist;
    }
    
    void UpdateMaglock() {
        var form = FindMaglockShip();

        if (maglockShip != null && form != maglockShip && hasAuthority)
            StopMaglock();
        
        if (maglockShip == null && form != maglockShip && hasAuthority)
            SetMaglock(form);
        
        if (maglockShip != null) {
            currentBlockPos = maglockShip.WorldToBlockPos(transform.position);
            currentBlock = maglockShip.blocks[currentBlockPos, BlockLayer.Base];
        }

        if (maglockShip != null)
            UpdateMaglockMove();
    }
    
    void Update() {
        UpdateMaglock();
    }

    void Start() {        
        //AddRigid();
        constructor = Pool.For("Constructor").Attach<Constructor>(transform);
    }
}
