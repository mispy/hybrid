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
        writer.Write(maglockMoveBlockPos);
    }

    public override void OnDeserialize(ExtendedBinaryReader reader, bool initial) {
        maglockMoveBlockPos = reader.ReadIntVector2();
    }

    public void TakeDamage(int amount) {
        health -= amount;
        
        if (health <= 0)
            Pool.Recycle(this.gameObject);
    }

    void AddRigid() {
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.drag = 2f;
        rigidBody.freezeRotation = true;
        syncRigid = gameObject.AddComponent<SyncRigid>();
        syncRigid.guid = new GUID(guid.ToString() + ":SyncRigid");
        SpaceNetwork.Register(syncRigid);
    }

    void Awake() {
        collider = gameObject.GetComponent<BoxCollider>();
        weapon = gameObject.AddComponent<CrewWeapon>();
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
        Destroy(syncRigid);
        Destroy(rigidBody);
        MaglockMove(ship.WorldToBlockPos(transform.position));
        collider.isTrigger = true;
        
        //if (this == Crew.player)
        //    maglockShip.blueprint.blocks.EnableRendering();
    }
    
    void StopMaglock() {
        transform.SetParent(Game.activeSector.contents);
        maglockShip.maglockedCrew.Remove(this);
        AddRigid();
        //if (this == Crew.player)
        //    maglockShip.blueprint.blocks.DisableRendering();
        collider.isTrigger = false;
        maglockShip = null;
        currentBlock = null;
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
		if (!maglockShip.blocks.IsPassable(maglockMoveBlockPos))
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
        
        if (maglockShip != null && form != maglockShip)
            StopMaglock();
        
        if (maglockShip == null && form != maglockShip)
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
        AddRigid();
        constructor = Pool.For("Constructor").Attach<Constructor>(transform);

        guid = new GUID("player" + GetComponent<NetworkIdentity>().netId.Value.ToString());
        SpaceNetwork.Register(this);
    }
}
