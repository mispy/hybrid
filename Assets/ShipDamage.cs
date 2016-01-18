using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipDamage : PoolBehaviour {
    Blockform form;
    BlockMap blocks;    
    HashSet<IntVector2> breaksToCheck = new HashSet<IntVector2>();

    void Awake() {
        form = GetComponent<Blockform>();
    }

    void OnEnable() {
        blocks = form.blocks;
        blocks.OnBlockRemoved += OnBlockRemoved;
        blocks.OnHealthUpdate += OnHealthUpdate;
        InvokeRepeating("UpdateBreaks", 0f, 0.1f);
    }

    void OnDisable() {
        blocks.OnBlockRemoved -= OnBlockRemoved;
        blocks.OnHealthUpdate -= OnHealthUpdate;
    }

    public HashSet<Block> blocksToUpdate = new HashSet<Block>();

    public override void OnSerialize(MispyNetworkWriter writer, bool initial) {
        if (initial) {
            var damaged = new List<Block>();
            foreach (var block in form.blocks.allBlocks)               
                if (block.health < block.type.maxHealth)
                    damaged.Add(block);

            writer.Write(damaged.Count);
            foreach (var block in damaged) {
                writer.Write(block.blockPos);
                writer.Write(block.health);
            }
        } else {
            writer.Write(blocksToUpdate.Count);
            foreach (var block in blocksToUpdate) {
                writer.Write(block.blockPos);
                writer.Write(block.health);
            }

            blocksToUpdate.Clear();
        }
    }

    public override void OnDeserialize(MispyNetworkReader reader, bool initial) {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++) {
            var pos = reader.ReadIntVector3();
            var health = reader.ReadSingle();
            var block = form.blocks[pos];
            if (block != null) {
                block.health = health;
            }
        }
    }

    void OnBlockRemoved(Block oldBlock) {        
        if (oldBlock.layer == BlockLayer.Base)
            breaksToCheck.Remove(oldBlock.pos);
    }

    public void OnHealthUpdate(Block block) {        
        if (block.health == 0) {
            BreakBlock(block);
            return;
        }

        if (block.gameObject == null && block.health == block.type.maxHealth)
            return;
        else if (block.gameObject == null)
            block.ship.RealizeBlock(block);

        var healthBar = block.gameObject.GetComponent<BlockHealthBar>();
        if (healthBar != null && block.health == block.type.maxHealth) {
            Destroy(healthBar);
            return;
        }           

        if (healthBar == null) {
            healthBar = block.gameObject.AddComponent<BlockHealthBar>();
            healthBar.block = block;
        }
    
        healthBar.OnHealthUpdate();
    }

    public void BreakBlock(Block block, bool checkBreaks = true) {
        //blocks[block.pos, block.layer] = null;

        if (block.layer == BlockLayer.Base) {
            var top = blocks[block.pos, BlockLayer.Top];
            if (top != null)
                BreakBlock(top);
            
            if (checkBreaks) {
                foreach (var neighbor in IntVector2.Neighbors(block.pos)) {
                    if (blocks[neighbor, BlockLayer.Base] != null)
                        breaksToCheck.Add(neighbor);
                }
            }           
        }
        
        
        /*var newShipObj = Pool.For("Ship").TakeObject();
        newShipObj.transfo`rm.position = BlockToWorldPos(block.pos);
        var newShip = newShipObj.GetComponent<Ship>();
        newShip.blocks[0, 0] = block;
        newShipObj.SetActive(true);
        newShip.rigidBody.velocity = rigidBody.velocity;
        newShip.rigidBody.angularVelocity = rigidBody.angularVelocity;*/
        //newShip.hasCollision = false;

        var item = Pool.For("Item").Attach<Rigidbody>(Game.activeSector.transients);
        item.position = form.BlockToWorldPos(block.pos);
        item.velocity = form.rigidBody.velocity;
        item.angularVelocity = form.rigidBody.angularVelocity;
        
        if (blocks.baseSize == 0) Pool.Recycle(gameObject);
    }
    
    public void BreakDetected(HashSet<Block> frag1, HashSet<Block> frag2) {
        var bigger = frag1.Count > frag2.Count ? frag1 : frag2;
        var smaller = frag1.Count > frag2.Count ? frag2 : frag1;
        
        foreach (var block in smaller) {
            foreach (var layerBlock in blocks.BlocksAtPos(block.pos)) {
                BreakBlock(layerBlock);
            }
        }
    }
    
    public void UpdateBreaks() {
        Profiler.BeginSample("UpdateBreaks");
        
        while (breaksToCheck.Count > 0) {
            var breakPos = breaksToCheck.First();
            breaksToCheck.Remove(breakPos);
            
            BlockBitmap fill = BlockPather.Floodfill(blocks, breakPos);
            
            if (fill.size == blocks.baseSize) {
                // There are no breaks here
                breaksToCheck.Clear();
            } else {
                if (fill.size < blocks.baseSize/2f) {
                    foreach (var pos in fill)
                        BreakBlock(blocks[pos, BlockLayer.Base], checkBreaks: false);
                } else {
                    foreach (var pos in fill)
                        breaksToCheck.Remove(pos);
                }
            }
        }
        
        Profiler.EndSample();
    }

}
