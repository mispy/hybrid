using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShipControl : MonoBehaviour {
    Blockform ship {
        get { return Game.playerShip; }
    }
    Transform selector;
    CrewBody selectedCrew = null;
    public Console console = null;
    public Blockform selectedShip { get; private set; }

    public HashSet<Block> selectedBlocks = new HashSet<Block>();
    public Dictionary<Block, Transform> blockSelectors = new Dictionary<Block, Transform>();

    void DeselectCrew() {
        if (selectedCrew == null) return;

        selector.gameObject.SetActive(false);
        selectedCrew = null;
    }

    void SelectCrew(CrewBody crew) {
        if (selector == null) {
            selector = Pool.For("Selector").Attach<Transform>(transform);
        }

        selector.transform.position = crew.transform.position;
        selector.transform.rotation = crew.transform.rotation;
        selector.transform.SetParent(crew.transform);
        selector.GetComponent<SpriteRenderer>().color = Color.green;
        selector.gameObject.SetActive(true);
        selectedCrew = crew;
#if UNITY_EDITOR
        Selection.activeGameObject = crew.gameObject;
#endif
     }

    void DeselectBlock(Block block) {
        if (block.gameObject != null) {
            block.gameObject.SendMessage("OnBlockDeselected", SendMessageOptions.DontRequireReceiver);
            Pool.Recycle(blockSelectors[block].gameObject);
        }

        Pool.Recycle(blockSelectors[block].gameObject);
        blockSelectors.Remove(block);
        selectedBlocks.Remove(block);
    }

    public void DeselectBlocks() {
        foreach (var block in selectedBlocks.ToList()) {
            DeselectBlock(block);
        }
    }

    public void SelectBlock(Block block) {
        if (selectedBlocks.Contains(block)) return;

        var selector = Pool.For("Selector").Attach<Transform>(transform);

        var worldPos = block.ship.BlockToWorldPos(block);
        selector.transform.position = worldPos;
        selector.transform.rotation = block.ship.transform.rotation;
        selector.transform.SetParent(block.ship.transform);
        selector.transform.localScale = new Vector2(block.Width*Tile.worldSize, block.Height*Tile.worldSize);
        selector.GetComponent<SpriteRenderer>().color = Color.green;

        selectedBlocks.Add(block);
        blockSelectors[block] = selector;

        if (block.gameObject != null)
            block.gameObject.SendMessage("OnBlockSelected", SendMessageOptions.DontRequireReceiver);

#if UNITY_EDITOR
        if (block.gameObject != null)
            Selection.activeGameObject = block.gameObject;
#endif
        Game.abilityMenu.gameObject.SetActive(true);
        Game.abilityMenu.OnBlockSelectionUpdate();
    }

    void HandleDoubleClick() {
        foreach (var block in ship.BlocksAtWorldPos(Game.mousePos)) {
            if (selectedBlocks.Contains(block)) {
                foreach (var comrade in ship.blocks.Find(block.type))
                    SelectBlock(comrade);
            }
        }
    }
    
    void HandleLeftClick() {
        DeselectBlocks();
        DeselectCrew();

        var form = Blockform.AtWorldPos(Game.mousePos);
            
        var blockPos = ship.WorldToBlockPos(Game.mousePos);

        var block = ship.blocks.Topmost(blockPos);
        if (block != null) {
            SelectBlock(block);
        }

        Game.abilityMenu.OnBlockSelectionUpdate();
    }
    
    void OnRightClick() {
		if (selectedCrew == null) {
			//Debug.Log(ship.pather.PathBetween(ship.transform.position, Game.mousePos));
			//Debug.Log(ship.BlocksAtWorldPos(Game.mousePos).First());
		}
    }

    float lastLeftClick = 0f;
    Vector2 lastLeftClickPos = new Vector2(0, 0);

    public void OnForwardThrust() {
        if (!console.canAccessThrusters) return;
        ship.FireThrusters(Facing.down);
    }

    public void OnReverseThrust() {        
        if (!console.canAccessThrusters) return;
        ship.FireThrusters(Facing.up);
    }

    public void OnStrafeLeft() {
        if (!console.canAccessThrusters) return;
        ship.FireThrusters(Facing.right);
    }

    public void OnStrafeRight() {
        if (!console.canAccessThrusters) return;
        ship.FireThrusters(Facing.left);
    }

    public void OnTurnRight() {
        if (!console.canAccessThrusters) return;
        ship.FireAttitudeThrusters(Facing.left);
    }

    public void OnTurnLeft() {
        if (!console.canAccessThrusters) return;
        ship.FireAttitudeThrusters(Facing.right);
    }

    public void OnLeftClick() {
        if (Time.time - lastLeftClick < 0.5f && Vector2.Distance(Input.mousePosition, lastLeftClickPos) < 0.5f)
            HandleDoubleClick();
        else
            HandleLeftClick();
        lastLeftClick = Time.time;
        lastLeftClickPos = Input.mousePosition;
    }

	void OnEnable() {
//        Game.mainCamera.orthographicSize = 32;

        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
        InputEvent.For(MouseButton.Right).Bind(this, OnRightClick);
        InputEvent.For(Keybind.ForwardThrust).Bind(this, OnForwardThrust, true);
        InputEvent.For(Keybind.ReverseThrust).Bind(this, OnReverseThrust, true);
        InputEvent.For(Keybind.StrafeLeft).Bind(this, OnStrafeLeft, true);
        InputEvent.For(Keybind.StrafeRight).Bind(this, OnStrafeRight, true);
        InputEvent.For(Keybind.TurnLeft).Bind(this, OnTurnLeft, true);
        InputEvent.For(Keybind.TurnRight).Bind(this, OnTurnRight, true);       
        InputEvent.For(Keybind.ToggleDesigner).Bind(this, OnToggleDesigner);
        InputEvent.For(Keybind.Jump).Bind(this, OnJump);
        InputEvent.For(KeyCode.Tab).Bind(this, OnAfterburn);

        InputEvent.For(KeyCode.Space).Bind(this, OnExitControl);
        DeselectBlocks();
	}
        
    public void OnJump() {
        Game.cameraControl.Lock(null);
        Game.mainCamera.transform.SetParent(Game.activeSector.transform);
        //ship.rigidBody.detectCollisions = false;
        ship.rigidBody.velocity = ship.transform.up * 1000; 
        //Game.fadeOverlay.FadeOut(1f);
        foreach (var block in Game.playerShip.blocks.allBlocks) {
            block.health = block.type.maxHealth;
        }
        Invoke("EndJump", 0.5f);
    }

    void EndJump() {
        Game.activeSector.Unload();
        Game.activeSector.gameObject.SetActive(false);
        Game.jumpMap.gameObject.SetActive(true);
        Game.Save();
        Game.playerShip.rigidBody.angularVelocity = Vector3.zero;
        Game.playerShip.transform.rotation = Quaternion.AngleAxis(0, Vector2.up);
        //Game.fadeOverlay.FadeIn(1f);
    }

    public void OnToggleDesigner() {
        if (Game.shipDesigner.gameObject.activeInHierarchy) {
            Game.shipDesigner.gameObject.SetActive(false);
        } else {
            Game.shipDesigner.gameObject.SetActive(true);
        }
    }

    public void OnExitControl() {
        console.crew = null;
        Game.shipControl.gameObject.SetActive(false);
        Game.crewControl.gameObject.SetActive(true);
    }

    public void OnAfterburn() {
/*        var inertia = ship.GetBlockComponents<InertiaNegator>().First();
        var power = inertia.GetComponent<PowerReceiver>();
        power.isReceiving = !power.isReceiving;*/
        foreach (var thruster in ship.GetBlockComponents<Thruster>()) {
            thruster.Afterburn();
        }
    }

    // Update is called once per frame
    void Update () {
        if (Game.playerShip == null) {
            gameObject.SetActive(false);
            return;
        }

		
		
		//Game.MoveCamera(Game.playership.transform.position);

		/*if (Game.activeSector.IsOutsideBounds(Game.playership.transform.position)) {
			Game.leaveSectorMenu.SetActive(true);
		} else {
			Game.leaveSectorMenu.SetActive(false);
		}*/


        var rigid = ship.rigidBody;
        
        if (Input.GetKey(KeyCode.X)) {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }

        foreach (var block in selectedBlocks.ToList()) {
            if (block.isBlueprint) {
                DeselectBlock(block);
                Game.abilityMenu.OnBlockSelectionUpdate();
            }
                
        }
    }
}