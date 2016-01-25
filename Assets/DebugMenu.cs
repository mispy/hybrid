using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class DebugMenu : MonoBehaviour {
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
        var crew = Pool.For("Crew").Attach<CrewBody>(Game.activeSector.contents);
        crew.transform.position = pos;
    }

    public void SpawnEnemy() {
        if (Blockform.AtWorldPos(Game.mousePos) == null) {
            var ship = Blockform.FromTemplate(ShipTemplate2.FromId("Little Frigate"));
            ship.rigidBody.position = Game.mousePos;
            SpawnCrew(Game.mousePos);
        }
    }

	public void ToggleVisibility() {
		Game.debugVisibility = !Game.debugVisibility;
		foreach (var form in Game.activeSector.blockforms) {
			form.fog.UpdateVisibility();
		}
	}

    public void RepairShip() {
        foreach (var block in Game.playerShip.blocks.allBlocks) {
            if (block.health != block.type.maxHealth) {                
                block.health = block.type.maxHealth;
            } else {
                block.health /= 2.0f;
            }
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

    public void EmptyInventory() {
        foreach (var type in BlockType.All) {
            Game.inventory[type] = 0;
        }
    }

    void OnEnable() {
        InputEvent.For(KeyCode.E).Bind(this, SpawnEnemy);
        InputEvent.For(KeyCode.V).Bind(this, ToggleVisibility);
        InputEvent.For(KeyCode.C).Bind(this, ControlShip);
        InputEvent.For(KeyCode.R).Bind(this, RepairShip);
        InputEvent.For(KeyCode.I).Bind(this, EmptyInventory);
    }
}
