using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipNameOverlay : MonoBehaviour {
    Ship ship;
    Text shipName;
	Text shipFaction;
    Canvas canvas;

    void Start () {
        canvas = GetComponentInParent<Canvas>();
        ship = GetComponentInParent<Blockform>().ship;
        shipName = transform.Find("ShipName").GetComponent<Text>();
		shipFaction = transform.Find("ShipFaction").GetComponent<Text>();
    }
    
    void Update () {
        shipName.color = ship.DispositionTowards(Game.playerShip).color;
		shipFaction.color = ship.faction == null ? Color.white : ship.faction.color;

        shipName.fontSize = Mathf.CeilToInt(Mathf.Log(ship.blocks.baseSize)*0.5f);
		shipFaction.fontSize = Mathf.CeilToInt(Mathf.Log(ship.blocks.baseSize)*0.5f);

		if (Game.playerShip == null)
			canvas.transform.rotation = Game.mainCamera.transform.rotation;
		else
	        canvas.transform.rotation = Game.playerShip.form.transform.rotation;

        transform.localPosition = Vector3.down * Mathf.Abs(ship.form.box.bounds.extents.y);

        shipName.text = ship.name;
		shipFaction.text = ship.faction == null ? "Independent" : ship.faction.name;
    }
}
