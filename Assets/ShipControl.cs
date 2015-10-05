using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipControl : MonoBehaviour {
	public static GameObject leaveSectorMenu;
	public static WeaponSelect weaponSelect;

    Ship ship;
    GameObject selector;
    Crew selectedCrew = null;

    void SelectCrew(Crew crew) {
        if (selector == null) {
            selector = Pool.For("Selector").TakeObject();
            selector.transform.parent = transform;
            selector.SetActive(true);
        }

        selector.transform.position = crew.body.transform.position;
        selector.transform.rotation = crew.body.transform.rotation;
        selector.transform.SetParent(crew.body.transform);
        selector.transform.localScale = new Vector3(1, 1, 1);
        selector.GetComponent<SpriteRenderer>().color = Color.green;
        selectedCrew = crew;
     }

	void FireWeapon(BlockType type) {
		foreach (var block in ship.blocks.Find(type)) {
			block.gameObject.GetComponent<ProjectileLauncher>().Fire();
		}
	}

    void HandleLeftClick() {
		if (weaponSelect.selectedType != null)
			FireWeapon(weaponSelect.selectedType);

        var blockPos = ship.form.WorldToBlockPos(Game.mousePos);
        foreach (var crew in ship.crew) {
            if (crew.body.currentBlockPos == blockPos) {
                SelectCrew(crew);
            }
        }
    }

    void HandleRightClick() {
		if (selectedCrew == null) {
			//Debug.Log(ship.form.pather.PathBetween(ship.form.transform.position, Game.mousePos));
			Debug.Log(ship.form.BlocksAtWorldPos(Game.mousePos).First());
		}

        if (selectedCrew != null) {
            selectedCrew.job = new MoveJob(ship.form.WorldToBlockPos(Game.mousePos));
        }
    }

    void HandleShipInput() {                
        var rigid = ship.form.rigidBody;    

        
        if (Input.GetKey(KeyCode.W)) {
            ship.form.FireThrusters(Orientation.down);        
        }
        
        if (Input.GetKey(KeyCode.S)) {
            ship.form.FireThrusters(Orientation.up);
        }
        
        if (Input.GetKey(KeyCode.A)) {
            ship.form.FireAttitudeThrusters(Orientation.right);
        }
        
        if (Input.GetKey(KeyCode.D)) {
            ship.form.FireAttitudeThrusters(Orientation.left);
        }

        if (Input.GetMouseButton(0)) {
            HandleLeftClick();
        }

        if (Input.GetMouseButtonDown(1)) {
            HandleRightClick();
        }

        if (Input.GetMouseButton(0)) {
            var selected = Game.main.weaponSelect.selectedType;
        
            if (selected == null) {
            } else if (selected is TractorBeam) {
                ship.form.StartTractorBeam(Game.mousePos);
            }
        } else {
            ship.form.StopTractorBeam();
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

		if (Input.GetKeyDown(KeyCode.Space)) {
			if (Game.isPaused)
				Game.Unpause();
			else
				Game.Pause();
		}

		
		// Scroll zoom
		if (Input.GetKeyDown(KeyCode.Equals) && Game.mainCamera.orthographicSize > 4) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize >> 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		} else if (Input.GetKeyDown(KeyCode.Minus) && Game.mainCamera.orthographicSize < 64) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize << 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		}
		
		// Scroll zoom
		if (Input.GetAxis("Mouse ScrollWheel") > 0 && Game.mainCamera.orthographicSize > 4) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize >> 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		} else if (Input.GetAxis("Mouse ScrollWheel") < 0) {// && Game.mainCamera.orthographicSize < 64) {
			Game.mainCamera.orthographicSize = (int)Game.mainCamera.orthographicSize << 1;
			//Debug.Log(Game.mainCamera.orthographicSize);
		}


        
        /*if (currentShip) {
            Game.main.debugText.text = String.Format("Velocity: {0} {1}", currentShip.rigidBody.velocity.x, currentShip.rigidBody.velocity.y);
        }*/
    }

	void Start() {
		ShipControl.leaveSectorMenu = GameObject.Find("LeavingSector");
		ShipControl.weaponSelect = GetComponentInChildren<WeaponSelect>();
	}

    // Update is called once per frame
    void Update () {
        ship = Game.playerShip;
		
		
		Game.MoveCamera(Game.playerShip.form.transform.position);
		Game.mainCamera.transform.rotation = Game.playerShip.form.transform.rotation;

		if (Game.activeSector.IsOutsideBounds(Game.playerShip.form.transform.position)) {
			leaveSectorMenu.SetActive(true);
		} else {
			leaveSectorMenu.SetActive(false);
		}
		
		if (Game.inputBlocked) return;
        HandleShipInput();
    }
}