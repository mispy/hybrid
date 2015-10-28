using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class Ship : PoolBehaviour, IOpinionable {
    public static List<Ship> all = new List<Ship>();

    public static Ship Create(string template = null, Faction faction = null, Jumpable beacon = null) {
        if (template == null) template = "Little Frigate";
        if (faction == null) faction = Util.GetRandom(Faction.all);
        //if (sector == null) sector = Util.GetRandom(SectorManager.all);

        var ship = Pool.For("Ship").Attach<Ship>(Game.galaxy.shipHolder);

        for (var i = 0; i < 6; i++ ) {
            CrewManager.Create(ship: ship, faction: faction);
        }
//        if (sector != null)
//            sector.PlaceShip(ship, (Vector2)sectorPos);
//        else
            ship.galaxyPos = Game.galaxy.RandomPosition();

        Ship.all.Add(ship);
        return ship;
    }

    public static Ship FromTemplate(ShipTemplate2 template) {
        var ship = Pool.For("Ship").Attach<Ship>(Game.galaxy.shipHolder);

        ship.name = template.name;
        
        foreach (var block in template.blocks.allBlocks) {
            ship.blueprintBlocks[block.pos, block.layer] = new BlueprintBlock(block);
            ship.blocks[block.pos, block.layer] = new Block(block);
        }

        return ship;
    }

    public string nameWithColor {
        get { return name; }
    }

    public BlockMap blocks;
    public BlockMap blueprintBlocks;
    public List<Crew> crew = new List<Crew>();
    public float scrapAvailable = 0f;
    public float jumpSpeed = 10f;
    public GalaxyPos galaxyPos;
    public Vector2 sectorPos;
    public Jumpable jumpPos;
    public Jumpable jumpDest;
    public new string name;

    public Faction faction {
        get {
            return captain != null ? captain.faction : null;
        }
    }
    public Crew captain {
        get {
            if (!crew.Any())
                return null;
            return crew.First();
        }
    }

    [NonSerialized]
    public ShipStrategy strategy;
    public Blockform form = null;
    public JumpShip jumpShip = null;
    public Dictionary<Ship, Disposition> localDisposition = new Dictionary<Ship, Disposition>();


    public bool isStationary {
        get { return !blocks.Find<Thruster>().Any(); }
    }

    public bool inTransit {
        get { return jumpDest != null; }
    }

    public override void OnCreate() {
        strategy = new ShipStrategy(this);

        if (blocks == null)
            blocks = Pool.For("BlockMap").Attach<BlockMap>(transform);
        blocks.ship = this;
        if (blueprintBlocks == null)
            blueprintBlocks = Pool.For("BlockMap").Attach<BlockMap>(transform);
        blueprintBlocks.ship = this;
        blocks.OnBlockAdded += OnBlockAdded;
    }

    public Blockform LoadBlockform() {
        var blockform = Pool.For("Blockform").Attach<Blockform>(Game.activeSector.contents, false);
        blockform.Initialize(this);
        this.form = blockform;
        blockform.gameObject.SetActive(true);
        return blockform;
    }

    public void OnBlockAdded(Block newBlock) {
        newBlock.ship = this;
    }

    public void FoldJump(Jumpable jumpDest) {
        Debug.LogFormat("Jumping to: {0}", jumpDest);
        this.jumpDest = jumpDest;

/*        if (sector != null)
            sector.ships.Remove(this);
        sector = null;*/
    }

    public void Simulate(float deltaTime) {
        // don't simulate realized ships
        if (form != null) return;

        strategy.Simulate();

        if (jumpDest != null) {
            var targetDir = (jumpDest.galaxyPos - galaxyPos.vec).normalized;
            var dist = targetDir * jumpSpeed * deltaTime;
            
            if (Vector2.Distance(jumpDest.galaxyPos, galaxyPos) < dist.magnitude) {
                //destSector.JumpEnterShip(this, destSector.galaxyPos.vec - galaxyPos.vec);
            } else {
                galaxyPos = new GalaxyPos(null, galaxyPos.vec + dist);
            }
        }

        if (jumpShip != null) jumpShip.SyncShip();
    }
    
    public void SetBlock(IntVector2 pos, BlockType type) {
        var block = new Block(type);
        blocks[pos, block.layer] = block;
        var block2 = new BlueprintBlock(type);
        blueprintBlocks[pos, block2.layer] = block2;
    }
    
    public void SetBlock(int x, int y, BlockType type) {
        var block = new Block(type);
        blocks[x, y, block.layer] = block;
        var block2 = new BlueprintBlock(type);
        blueprintBlocks[x, y, block2.layer] = block2;
    }

    public Disposition DispositionTowards(Ship other) {
        if (localDisposition.ContainsKey(other))
            return localDisposition[other];

        if (captain == null) return Disposition.neutral;

        return Disposition.FromOpinion(captain.opinion[other]);
    }

}

