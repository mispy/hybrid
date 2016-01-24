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
        InputEvent.For(MouseButton.Left).Bind(this, OnRepair, true);
    }

    public void OnRepair() {
        foreach (var block in Block.AtWorldPos(Game.mousePos)) {
            if (!block.isDamaged) return;
            Annotation.DrawLine(crew.transform.position, block.gameObject.transform.position, Color.blue, 0.5f);
            block.health += 5*Time.deltaTime;
            break;
        }
    }


    public void OnToggleControl() {
        Game.playerShip = crew.maglockShip;
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
