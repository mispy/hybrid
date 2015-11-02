using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CrewBody : NetworkBehaviour {
    public Rigidbody rigidBody;
    public new BoxCollider collider;
    public Constructor constructor;
    
    
    // Crew can be maglocked to a ship even if they don't have a current block
    // bc they attach to the sides as well
    public Blockform maglockShip = null;
    public IntVector2 currentBlockPos;
    public IntVector2 maglockMoveBlockPos;
    public Block currentBlock = null;
    
    public Block controlConsole = null;
    
    public CrewWeapon weapon;
    [ReadOnlyAttribute]
    public CrewMind mind;

    public int maxHealth;
    public int health;

    public NetworkTransform netform { get; private set; }

    public void TakeDamage(int amount) {
        health -= amount;
        
        if (health <= 0)
            Pool.Recycle(this.gameObject);
    }

    void AddRigid() {
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.drag = 2f;
        rigidBody.freezeRotation = true;
    }

    void Awake() {
        collider = gameObject.AddComponent<BoxCollider>();
        weapon = gameObject.AddComponent<CrewWeapon>();
        mind = gameObject.AddComponent<CrewMind>();      
        AddRigid();
        netform = GetComponent<NetworkTransform>();
        netform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncRigidbody3D;

        gameObject.AddComponent<CrewControl>();

        constructor = Pool.For("Constructor").Attach<Constructor>(transform);
    }

    public override void OnStartLocalPlayer() {
        Game.localPlayer = this;
    }

    public void MaglockMove(IntVector2 bp) {
        maglockMoveBlockPos = bp;
    }
    
    public void UseBlock(Block block)  {
        controlConsole = block;
    }
    
    public void SetMaglock(Blockform ship) {
        maglockShip = ship;
        maglockShip.maglockedCrew.Add(this);
        transform.rotation = maglockShip.transform.rotation;
        transform.SetParent(maglockShip.transform);
        Destroy(rigidBody);
        netform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
		MaglockMove(ship.WorldToBlockPos(transform.position));
        
        //if (this == Crew.player)
        //    maglockShip.blueprint.blocks.EnableRendering();
    }
    
    void StopMaglock() {
        transform.SetParent(Game.activeSector.contents);
       // rigidBody.isKinematic = false;
        maglockShip.maglockedCrew.Remove(this);
        AddRigid();
        netform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncRigidbody3D;
        //if (this == Crew.player)
        //    maglockShip.blueprint.blocks.DisableRendering();
        
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

        var speed = 15f;
        var targetPos = (Vector3)maglockShip.BlockToLocalPos(maglockMoveBlockPos);
        
        var pos = transform.localPosition;
        var dist = targetPos - pos;
        
        if (dist.magnitude > speed*Time.deltaTime) {
            dist.Normalize();
            dist = dist*speed*Time.deltaTime;
        }
        
        transform.localPosition += dist;
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
}
