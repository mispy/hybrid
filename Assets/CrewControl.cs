using UnityEngine;
using System.Collections;

public class CrewControl : MonoBehaviour {
    CrewBody crew;

    public void OnEnable() {
        crew = GetComponentInParent<CrewBody>();

        InputEvent.For(KeyCode.W).Bind(this, OnMoveForward, true);
        InputEvent.For(KeyCode.A).Bind(this, OnMoveLeft, true);
        InputEvent.For(KeyCode.D).Bind(this, OnMoveRight, true);
        InputEvent.For(KeyCode.S).Bind(this, OnMoveDown, true);
        InputEvent.For(KeyCode.E).Bind(this, OnToggleControl);
    }

    public void OnToggleControl() {
        if (Game.shipControl.isActiveAndEnabled) { 
            Game.shipControl.gameObject.SetActive(false);
        } else {
            Game.playerShip = crew.maglockShip;
            Game.shipControl.gameObject.SetActive(true);
        }
    }

    public void OnMoveForward() {
        if (crew.isMaglocked)
            crew.MaglockMove(crew.currentBlockPos + IntVector2.up);
        else
            crew.rigidBody.velocity += transform.up;
    }

    public void OnMoveLeft() {
        if (crew.isMaglocked)
            crew.MaglockMove(crew.currentBlockPos + IntVector2.left);
        else
            crew.rigidBody.velocity += -transform.right;
    }

    public void OnMoveRight() {
        if (crew.isMaglocked)
            crew.MaglockMove(crew.currentBlockPos + IntVector2.right);
        else
            crew.rigidBody.velocity += transform.right;
    }

    public void OnMoveDown() {
        if (crew.isMaglocked)
            crew.MaglockMove(crew.currentBlockPos + IntVector2.down);
        else
            crew.rigidBody.velocity += -transform.up;
    }
}
