using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShipControl : MonoBehaviour {
    Ship ship;
    Transform selector;
    Crew selectedCrew = null;
    public Ship selectedShip { get; private set; }

    public HashSet<Block> selectedBlocks = new HashSet<Block>();
    public Dictionary<Block, Transform> blockSelectors = new Dictionary<Block, Transform>();

    void DeselectCrew() {
        if (selectedCrew == null) return;

        selector.gameObject.SetActive(false);
        selectedCrew = null;
    }

    void SelectCrew(Crew crew) {
        if (selector == null) {
            selector = Pool.For("Selector").Attach<Transform>(transform);
        }

        selector.transform.position = crew.body.transform.position;
        selector.transform.rotation = crew.body.transform.rotation;
        selector.transform.SetParent(crew.body.transform);
        selector.GetComponent<SpriteRenderer>().color = Color.green;
        selector.gameObject.SetActive(true);
        selectedCrew = crew;
     }

    void DeselectBlock(Block block) {
        if (block.type.isComplexBlock)
            block.gameObject.SendMessage("OnBlockDeselected", SendMessageOptions.DontRequireReceiver);

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

        var worldPos = block.ship.form.BlockToWorldPos(block);
        selector.transform.position = worldPos;
        selector.transform.rotation = block.ship.form.transform.rotation;
        selector.transform.SetParent(block.ship.form.transform);
        selector.transform.localScale = new Vector2(block.Width*Tile.worldSize, block.Height*Tile.worldSize);
        selector.GetComponent<SpriteRenderer>().color = Color.green;

        selectedBlocks.Add(block);
        blockSelectors[block] = selector;

        if (block.type.isComplexBlock)
            block.gameObject.SendMessage("OnBlockSelected", SendMessageOptions.DontRequireReceiver);


        Game.abilityMenu.gameObject.SetActive(true);
        Game.abilityMenu.OnBlockSelectionUpdate();
    }

    void SelectShip(Ship ship) {
        selectedShip = ship;
        Game.shipInfo.gameObject.SetActive(true);
    }

    void HandleDoubleClick() {
        foreach (var block in ship.form.BlocksAtWorldPos(Game.mousePos)) {
            if (selectedBlocks.Contains(block)) {
                foreach (var comrade in ship.form.blocks.Find(block.type))
                    SelectBlock(comrade);
            }
        }
    }
    
    void HandleLeftClick() {
        DeselectBlocks();
        DeselectCrew();

        var form = Blockform.AtWorldPos(Game.mousePos);

        if (form != null && form.ship != Game.playerShip)
            SelectShip(form.ship);
            
        var blockPos = ship.form.WorldToBlockPos(Game.mousePos);

        foreach (var crew in ship.crew) {
            if (crew.body.currentBlockPos == blockPos) {
                SelectCrew(crew);
                return;
            }
        }

        var block = ship.form.blocks.Topmost(blockPos);
        if (block != null) {
            SelectBlock(block);
        }

        Game.abilityMenu.OnBlockSelectionUpdate();
    }
    
    void OnRightClick() {
		if (selectedCrew == null) {
			//Debug.Log(ship.form.pather.PathBetween(ship.form.transform.position, Game.mousePos));
			//Debug.Log(ship.form.BlocksAtWorldPos(Game.mousePos).First());
		}

        if (selectedCrew != null) {
            selectedCrew.job = new MoveJob(ship.form.WorldToBlockPos(Game.mousePos));
        }
    }

    float lastLeftClick = 0f;
    Vector2 lastLeftClickPos = new Vector2(0, 0);

    public void OnForwardThrust() {
        ship.form.FireThrusters(Facing.down);        
    }

    public void OnReverseThrust() {
        ship.form.FireThrusters(Facing.up);
    }

    public void OnStrafeLeft() {
        ship.form.FireThrusters(Facing.right);
    }

    public void OnStrafeRight() {
        ship.form.FireThrusters((Facing.left));
    }

    public void OnTurnRight() {
        ship.form.FireAttitudeThrusters(Facing.left);
    }

    public void OnTurnLeft() {
        ship.form.FireAttitudeThrusters(Facing.right);
    }

    public void OnLeftClick() {
        if (Time.time - lastLeftClick < 0.5f && Vector2.Distance(Input.mousePosition, lastLeftClickPos) < 0.5f)
            HandleDoubleClick();
        else
            HandleLeftClick();
        lastLeftClick = Time.time;
        lastLeftClickPos = Input.mousePosition;
    }

    public void OnToggleDesigner() {
        if (Game.shipDesigner.gameObject.activeInHierarchy) {
            Game.shipDesigner.gameObject.SetActive(false);
        } else {
            Game.shipDesigner.gameObject.SetActive(true);
        }
    }

	void OnEnable() {
        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
        InputEvent.For(MouseButton.Right).Bind(this, OnRightClick);
        InputEvent.For(Keybind.ToggleDesigner).Bind(this, OnToggleDesigner);
        InputEvent.For(Keybind.ForwardThrust).Bind(this, OnForwardThrust, true);
        InputEvent.For(Keybind.ReverseThrust).Bind(this, OnReverseThrust, true);
        InputEvent.For(Keybind.StrafeLeft).Bind(this, OnStrafeLeft, true);
        InputEvent.For(Keybind.StrafeRight).Bind(this, OnStrafeLeft, true);
        InputEvent.For(Keybind.TurnLeft).Bind(this, OnTurnLeft, true);
        InputEvent.For(Keybind.TurnRight).Bind(this, OnTurnRight, true);
	}

    // Update is called once per frame
    void Update () {
        ship = Game.playerShip;
		
		
		//Game.MoveCamera(Game.playerShip.form.transform.position);

		/*if (Game.activeSector.IsOutsideBounds(Game.playerShip.form.transform.position)) {
			Game.leaveSectorMenu.SetActive(true);
		} else {
			Game.leaveSectorMenu.SetActive(false);
		}*/


        var rigid = ship.form.rigidBody;
        
        if (Input.GetKey(KeyCode.X)) {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        
        if (Input.GetKeyDown(KeyCode.J)) {
            JumpMap.Activate();
        }
    }
}