using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ShipInfo : MonoBehaviour {
    Ship ship;
    Text infoText;
    Button hailButton;

	// Use this for initialization
	void Start () {
	    infoText = transform.Find("InfoText").GetComponent<Text>();
        hailButton = transform.Find("HailButton").GetComponent<Button>();
        hailButton.onClick.AddListener(() => Game.dialogueMenu.StartDialogue(ship));
	}
	
	// Update is called once per frame
	void Update () {
	    ship = Game.shipControl.selectedShip;
        if (ship == null) {
            gameObject.SetActive(false);
            return;
        }

        var text = "";
        text += String.Format("<size=20>{0}</size>\n", ship.name);

        text += String.Format("<size=18><color='#{0}'>{1}</color></size>\n", ColorUtility.ToHtmlStringRGB(ship.faction.color), ship.faction.name);

        var disposition = ship.DispositionTowards(Game.playerShip);
        text += String.Format("<color='#{0}'>{1}</color>\n", ColorUtility.ToHtmlStringRGB(disposition.color), disposition);

        infoText.text = text;
	}
}
