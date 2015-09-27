using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipUI : MonoBehaviour {
    public Text shipName;
    void Update() {
        var ship = Game.playerShip;

        if (ship == null) {
            shipName.text = "";
            return;
        }

        shipName.text = ship.name;
    }
}
