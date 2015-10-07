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
        var friendlyColor = new Color(155/255f, 251/255f, 152/255f);
        var hostileColor = new Color(255/255f, 69/255f, 0f);

        if (Game.playerShip != null && Game.playerShip.faction.IsEnemy(ship.faction)) {
            shipName.color = hostileColor;
        } else {
            shipName.color = friendlyColor;
        }

		shipFaction.color = ship.faction.color;

        shipName.fontSize = Mathf.CeilToInt(Mathf.Log(ship.blocks.baseSize)*0.5f);
		shipFaction.fontSize = Mathf.CeilToInt(Mathf.Log(ship.blocks.baseSize)*0.5f);

		if (Game.playerShip == null)
			canvas.transform.rotation = Game.mainCamera.transform.rotation;
		else
	        canvas.transform.rotation = Game.playerShip.form.transform.rotation;

        shipName.text = ship.name;
		shipFaction.text = ship.faction.name;
    }
}
