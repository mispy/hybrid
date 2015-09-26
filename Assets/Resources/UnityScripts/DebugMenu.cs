﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class DebugMenu : MonoBehaviour {
    public void SaveShip() {
        var ship = Game.playerShip;
        if (ship == null) return;
        var data = ShipManager.Pack(ship);
        var path = Application.dataPath + "/Ships/New/" + ship.name + ".xml";
        Save.Dump(data, path);
        Game.main.BriefMessage("Saved " + path);
    }

    public void NewShip() {
        var ship = Game.playerShip;
        foreach (var block in ship.blocks.AllBlocks) {        
            ship.blocks[block.pos, block.layer] = null;
            ship.blueprintBlocks[block.pos, block.layer] = null;
        }
        ship.SetBlock<Floor>(0, 0);
    }

    public void MakeAsteroid(Vector2 pos) {                
        for (var i = 0; i < 10; i++) {
            var radius = Random.Range(5, 10);
            if (Physics.OverlapSphere(pos, radius*Tile.worldSize).Length == 0) {
                Generate.Asteroid(pos, radius);
                break;
            }
        }
    }

    public void SpawnCrew(Vector2 pos) {
        var crewObj = Pool.For("CrewBody").TakeObject();
        crewObj.transform.position = pos;
        crewObj.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
        Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SaveShip();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            NewShip();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            if (Blockform.AtWorldPos(pz) == null) {
                var ship = ShipManager.Create(sector: Game.playerShip.sector);
                Game.activeSector.RealizeShip(ship, pz);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            MakeAsteroid(pz);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            SpawnCrew(pz);
        }
    }
}