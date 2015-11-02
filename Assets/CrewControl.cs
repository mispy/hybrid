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
    }

    public void OnMoveForward() {
        crew.rigidBody.velocity += transform.up;
    }

    public void OnMoveLeft() {
        crew.rigidBody.velocity += -transform.right;
    }

    public void OnMoveRight() {
        crew.rigidBody.velocity += transform.right;
    }

    public void OnMoveDown() {
        crew.rigidBody.velocity += -transform.up;
    }
}
