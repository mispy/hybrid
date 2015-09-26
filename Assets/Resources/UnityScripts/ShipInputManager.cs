using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipInputManager : MonoBehaviour {
	void HandleShipInput() {				
		var ship = Game.playerShip.form;

		Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		
		var rigid = ship.rigidBody;	
		
		if (Input.GetKey(KeyCode.W)) {
			ship.FireThrusters(Orientation.down);		
		}
		
		if (Input.GetKey(KeyCode.S)) {
			ship.FireThrusters(Orientation.up);
		}
		
		if (Input.GetKey(KeyCode.A)) {
			ship.FireAttitudeThrusters(Orientation.right);
		}
		
		if (Input.GetKey(KeyCode.D)) {
			ship.FireAttitudeThrusters(Orientation.left);
		}
		
		if (Input.GetMouseButton(0)) {
			var selected = Game.main.weaponSelect.selectedType;
		
			if (selected == null) {
			} else if (selected is TractorBeam) {
				ship.StartTractorBeam(pz);
			}
		} else {
			ship.StopTractorBeam();
		}
		
		if (Input.GetKey(KeyCode.X)) {
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero;
		}

		if (Input.GetKeyDown(KeyCode.F1)) {
			if (Game.main.shipDesigner.gameObject.activeInHierarchy) {
				Game.main.shipDesigner.gameObject.SetActive(false);
			} else {
				Game.main.shipDesigner.gameObject.SetActive(true);
			}
		}

		if (Input.GetKeyDown(KeyCode.J)) {
			JumpMap.Activate();
		}

		
		/*if (currentShip) {
			Game.main.debugText.text = String.Format("Velocity: {0} {1}", currentShip.rigidBody.velocity.x, currentShip.rigidBody.velocity.y);
		}*/
	}

	// Update is called once per frame
	void Update () {
		if (Game.inputBlocked) return;
		HandleShipInput();
	}
}