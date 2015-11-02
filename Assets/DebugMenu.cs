using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class DebugMenu : MonoBehaviour {
    public void NewShip() {
        var ship = Game.playerShip;
		foreach (var pos in ship.blocks.FilledPositions) {
			ship.blocks[pos, BlockLayer.Base] = null;
			ship.blueprint.blocks[pos, BlockLayer.Base] = null;
        }
       // ship.SetBlock(0, 0, BlockType.FromId("Floor"));
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
/*        if (Blockform.AtWorldPos(Game.mousePos) == null) {
            Ship.Create(beacon: Game.playerShip.beacon, faction: Faction.FromId("Pirate Gang"));
        }*/
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
            Game.playerShip = form;
            
            foreach (var f in Game.activeSector.blockforms) {
                f.fog.UpdateVisibility();
            }
        }
    }

    void Start() {
        InputEvent.For(KeyCode.N).Bind(this, NewShip);
        InputEvent.For(KeyCode.E).Bind(this, SpawnEnemy);
        InputEvent.For(KeyCode.V).Bind(this, ToggleVisibility);
        InputEvent.For(KeyCode.C).Bind(this, ControlShip);
    }
}
