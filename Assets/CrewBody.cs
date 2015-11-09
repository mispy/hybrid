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
    [SyncVar]
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

    public NetworkTransform netform { get; private set; }
    
    public void NewSyncPos(Vector2 pos) {
        transform.localPosition = pos;
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
    }

    void Awake() {
        collider = gameObject.GetComponent<BoxCollider>();
        weapon = gameObject.AddComponent<CrewWeapon>();
        //mind = gameObject.AddComponent<CrewMind>();      

        AddRigid();
        netform = GetComponent<NetworkTransform>();
        constructor = Pool.For("Constructor").Attach<Constructor>(transform);
    }

    public override void OnStartLocalPlayer() {
        Game.localPlayer = this;        
        gameObject.AddComponent<CrewControl>();
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
        netform.enabled = false;
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
        netform.enabled = true;
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
}
