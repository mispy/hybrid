using UnityEngine;
using System.Collections;

public class CrewControl : MonoBehaviour {
    CrewBody crew {
        get { return Game.localPlayer.crew; }
    }

    public void OnEnable() {
//        Game.mainCamera.orthographicSize = 4;

        InputEvent.For(KeyCode.W).Bind(this, OnMoveForward, true);
        InputEvent.For(KeyCode.A).Bind(this, OnMoveLeft, true);
        InputEvent.For(KeyCode.D).Bind(this, OnMoveRight, true);
        InputEvent.For(KeyCode.S).Bind(this, OnMoveDown, true);
        InputEvent.For(KeyCode.Space).Bind(this, OnToggleControl);
        InputEvent.For(MouseButton.Left).Bind(this, OnFireWeapon, true);
        InputEvent.For(MouseButton.Right).Bind(this, OnRepair, true);
    }

    public void OnRepair() {
        crew.repairTool.Repair(Game.mousePos);
    }

    public void OnFireWeapon() {
        crew.GetComponentInChildren<CrewBeamWeapon>().Fire(Game.mousePos);
    }


    public void OnToggleControl() {
        var console = crew.currentBlock.GetBlockComponent<Console>();
        if (console == null) return;

        Game.playerShip = crew.maglockShip;
        console.crew = crew;
        Game.shipControl.console = console;
        Game.shipControl.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnMoveForward() {
        if (crew.isMaglocked) {
            crew.MaglockMove(crew.currentBlockPos + IntVector2.up);
        } else
            crew.rigidBody.velocity += crew.transform.up;
    }

    public void OnMoveLeft() {
        if (crew.isMaglocked) {
            crew.MaglockMove(crew.currentBlockPos + IntVector2.left);
        } else
            crew.rigidBody.velocity += -crew.transform.right;
    }

    public void OnMoveRight() {
        if (crew.isMaglocked) {
            crew.MaglockMove(crew.currentBlockPos + IntVector2.right);
        } else
            crew.rigidBody.velocity += crew.transform.right;
    }

    public void OnMoveDown() {
        if (crew.isMaglocked) {
            crew.MaglockMove(crew.currentBlockPos + IntVector2.down);
        } else
            crew.rigidBody.velocity += -crew.transform.up;
    }
}
