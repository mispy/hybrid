using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InputListener {
    public MonoBehaviour comp;

    public InputListener(MonoBehaviour comp) {
        this.comp = comp;
    }
}

public class InputEvent {
    public static InputEvent OnLeftClick = new InputEvent("OnLeftClick");
    public static InputEvent OnMiddleClick = new InputEvent("OnMiddleClick");
    public static InputEvent OnRightClick = new InputEvent("OnRightClick");
    public static InputEvent OnForwardThrust = new InputEvent("OnForwardThrust");
    public static InputEvent OnReverseThrust = new InputEvent("OnReverseThrust");
    public static InputEvent OnStrafeLeft = new InputEvent("OnStrafeLeft");
    public static InputEvent OnStrafeRight = new InputEvent("OnStrafeRight");
    public static InputEvent OnTurnRight = new InputEvent("OnTurnRight");
    public static InputEvent OnTurnLeft = new InputEvent("OnTurnLeft");
    public static InputEvent OnToggleDesigner = new InputEvent("OnToggleDesigner");

    public static void Update() {
        if (Input.GetMouseButtonDown(0))
            OnLeftClick.Trigger();
        
        if (Input.GetMouseButtonDown(1))
            OnRightClick.Trigger();
        
        if (Input.GetMouseButtonDown(2))
            OnMiddleClick.Trigger();

        if (Input.GetKey(KeyCode.W))
            OnForwardThrust.Trigger();
        
        if (Input.GetKey(KeyCode.S))
            OnReverseThrust.Trigger();
        
        if (Input.GetKey(KeyCode.Q))
            OnStrafeLeft.Trigger();

        if (Input.GetKey(KeyCode.E))
            OnStrafeRight.Trigger();
        
        if (Input.GetKey(KeyCode.A))
            OnTurnLeft.Trigger();
        
        if (Input.GetKey(KeyCode.D))
            OnTurnRight.Trigger();
               
        if (Input.GetKeyDown(KeyCode.F1))
            OnToggleDesigner.Trigger();
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (Game.isPaused)
                Game.Unpause();
            else
                Game.Pause();
        }
    }

    string name;
    List<InputListener> listeners = new List<InputListener>();

    public InputEvent(string name) {
        this.name = name;
    }

    public void AddListener(MonoBehaviour comp) {
        var listener = new InputListener(comp);
        listeners.Add(listener);
    }

    public void Trigger() {
        for (var i = listeners.Count-1; i >= 0; i--) {
            var listener = listeners[i];
            if (listener.comp.gameObject.activeInHierarchy) {
                listener.comp.SendMessage(name);
                break;
            } 
        }

        CleanInactive();
    }

    public void CleanInactive() {
        var toKeep = new List<InputListener>();
        foreach (var listener in listeners) {
            if (listener.comp.gameObject.activeInHierarchy)
                toKeep.Add(listener);
        }

        listeners = toKeep;
    }
}
