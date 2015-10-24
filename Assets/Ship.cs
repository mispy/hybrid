using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class ShipTemplate : ISaveBindable {
    public static Dictionary<string, ShipTemplate> byId = new Dictionary<string, ShipTemplate>();

    public static void LoadAll() {
        foreach (var path in Directory.GetFiles(Application.dataPath + "/Ships/", "*.xml")) {
            var template = Save.Load<ShipTemplate>(path);
            var id = Util.GetIdFromPath(path);
            template.name = id;
            byId[id] = template;                        
        }
    }

    public static ShipTemplate FromId(string id) {
        return byId[id];
    }

    public string name;
    public List<Block> blocks;

    public void Savebind(ISaveBinder save) {
        save.BindList("blocks", ref blocks);
    }

    // Create a new template from an existing ship
    public ShipTemplate(Ship ship) {
        this.name = ship.name;
        this.blocks = ship.blueprintBlocks.allBlocks.ToList();
    }
}

public class Ship : IOpinionable, ISaveBindable {
    public static List<Ship> all = new List<Ship>();

    public static Ship Create(string template = null, Faction faction = null, Sector sector = null, Vector2? sectorPos = null) {
        if (template == null) template = "Little Frigate";
        if (faction == null) faction = Util.GetRandom(FactionManager.all);
        //if (sector == null) sector = Util.GetRandom(SectorManager.all);
        if (sector != null && sectorPos == null) sectorPos = sector.RandomEdge();
        
        var ship = new Ship(ShipTemplate.FromId(template));
        
        for (var i = 0; i < 6; i++ ) {
            CrewManager.Create(ship: ship, faction: faction);
        }
        if (sector != null)
            sector.PlaceShip(ship, (Vector2)sectorPos);
        else
            ship.galaxyPos = Game.galaxy.RandomPosition();

        Ship.all.Add(ship);
        return ship;
    }


    public string name;
    public string nameWithColor {
        get { return name; }
    }

    public BlockMap blocks;
    public BlockMap blueprintBlocks;
    public HashSet<Crew> crew;
    public float scrapAvailable = 0f;
    public float jumpSpeed = 10f;
    public GalaxyPos galaxyPos;
    public Vector2 sectorPos;
    public Sector sector;
    public Sector destSector;
    public Faction faction {
        get {
            return captain.faction;
        }
    }
    public Crew captain {
        get {
            return crew.First();
        }
    }

    public ShipStrategy strategy;
    public Blockform form = null;
    public JumpShip jumpShip = null;
    public Dictionary<Ship, Disposition> localDisposition = new Dictionary<Ship, Disposition>();

    public bool isStationary {
        get { return !blocks.Find<Thruster>().Any(); }
    }

    public bool inTransit {
        get { return destSector != null; }
    }

    public Ship() {
        crew = new HashSet<Crew>();
        strategy = new ShipStrategy(this);
        blocks = new BlockMap(this);
        blueprintBlocks = new BlockMap(this);
        blocks.OnBlockAdded += OnBlockAdded;
    }

    public Ship(ShipTemplate template) : this() {
        name = template.name;

        foreach (var block in template.blocks) {
            blueprintBlocks[block.pos, block.layer] = new BlueprintBlock(block);
            blocks[block.pos, block.layer] = new Block(block);
        }
    }

    public void Savebind(ISaveBinder save) {
        save.BindValue("name", ref name);
        save.BindSet("crew", ref crew);

        if (save is XMLSaveWriter) {
            save.BindSet("blocks", ref blocks.allBlocks);
            save.BindSet("blueprint", ref blueprintBlocks.allBlocks);
        } else {
            save.BindSet("blocks", ref blocks.allBlocks);
            save.BindSet("blueprint", ref blueprintBlocks.allBlocks);

            foreach (var block in blocks.allBlocks) {
                blocks[block.pos, block.layer] = block;
            }

            foreach (var blue in blueprintBlocks.allBlocks) {
                blueprintBlocks[blue.pos, blue.layer] = blue;
            }
        }
    }

    public Blockform LoadBlockform() {
        var blockform = Pool.For("Blockform").Attach<Blockform>(Game.activeSector.contents);
        blockform.Initialize(this);
        this.form = blockform;
        return blockform;
    }

    public void OnBlockAdded(Block newBlock) {
        newBlock.ship = this;
    }

    public void FoldJump(Sector destSector) {
        Debug.LogFormat("Jumping to: {0}", destSector);
        this.destSector = destSector;

        if (sector != null)
            sector.ships.Remove(this);
        sector = null;
    }

    public void Simulate(float deltaTime) {
        // don't simulate realized ships
        if (form != null) return;

        strategy.Simulate();

        if (destSector != null) {
            var targetDir = (destSector.galaxyPos.vec - galaxyPos.vec).normalized;
            var dist = targetDir * jumpSpeed * deltaTime;
            
            if (Vector2.Distance(destSector.galaxyPos, galaxyPos) < dist.magnitude) {
                destSector.JumpEnterShip(this, destSector.galaxyPos.vec - galaxyPos.vec);
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

