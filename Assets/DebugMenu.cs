using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class DebugMenu : MonoBehaviour {
    public void SaveTemplate() {
        var template = new ShipTemplate(Game.playerShip);
        Save.Write(template);
/*        var ship = Game.playerShip;
        if (ship == null) return;
        var data = ShipManager.Pack(ship);
        var path = Application.dataPath + "/Ships/" + ship.name + ".xml";
        Save.Dump(data, path);
        Game.main.BriefMessage("Saved " + path);*/
    }

    public void NewShip() {
        var ship = Game.playerShip;
		foreach (var pos in ship.blocks.FilledPositions) {
			ship.blocks[pos, BlockLayer.Base] = null;
			ship.blueprintBlocks[pos, BlockLayer.Base] = null;
        }
        ship.SetBlock(0, 0, BlockType.FromId("Floor"));
    }

    public void MakeAsteroid(Vector2 pos) {                
        for (var i = 0; i < 10; i++) {
            var radius = Random.Range(5, 10);
            if (Physics.OverlapSphere(pos, radius*Tile.worldSize).Length == 0) {
                //Generate.Asteroid(pos, radius);
                break;
            }
        }
    }

    public void SpawnCrew(Vector2 pos) {
        var crew = Pool.For("CrewBody").Attach<CrewBody>(Game.activeSector.contents);
        crew.transform.position = pos;
    }

    public void SpawnEnemy() {
        if (Blockform.AtWorldPos(Game.mousePos) == null) {
            Ship.Create(sector: Game.playerShip.sector, faction: FactionManager.byId["Pirate Gang"], sectorPos: Game.mousePos);
        }
    }

	public void ToggleVisibility() {
		Game.debugVisibility = !Game.debugVisibility;
		foreach (var form in Game.activeSector.blockforms) {
			form.fog.UpdateVisibility();
		}
	}

    public void ControlShip() {
        var form = Blockform.AtWorldPos(Game.mousePos);
        if (form != null) {
            Game.playerShip = form.ship;
            
            foreach (var f in Game.activeSector.blockforms) {
                f.fog.UpdateVisibility();
            }
        }
    }

    void Start() {
        InputEvent.For(KeyCode.S).Bind(this, SaveTemplate);
        InputEvent.For(KeyCode.N).Bind(this, NewShip);
        InputEvent.For(KeyCode.E).Bind(this, SpawnEnemy);
        InputEvent.For(KeyCode.V).Bind(this, ToggleVisibility);
        InputEvent.For(KeyCode.C).Bind(this, ControlShip);
    }
}
