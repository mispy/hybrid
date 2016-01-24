using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InputListener {
    public MonoBehaviour comp;
    public Action callback;
    public bool repeat;

    public InputListener() { }
    
    public InputListener(MonoBehaviour comp, Action callback, bool repeat) {
        this.comp = comp;
        this.callback = callback;
        this.repeat = repeat;
    }
}

public class InputListener<T> : InputListener {
    public new Action<T> callback;

    public InputListener(MonoBehaviour comp, Action<T> callback, bool repeat) {
        this.comp = comp;
        this.callback = callback;
        this.repeat = repeat;
    }
}

public static class Keybind {
    public static KeyCode ForwardThrust = KeyCode.W;
    public static KeyCode ReverseThrust = KeyCode.S;
    public static KeyCode StrafeLeft = KeyCode.Q;
    public static KeyCode StrafeRight = KeyCode.E;
    public static KeyCode TurnRight = KeyCode.D;
    public static KeyCode TurnLeft = KeyCode.A;
    public static KeyCode ToggleDesigner = KeyCode.F1;
    public static KeyCode Jump = KeyCode.J;
}

public enum MouseButton {
   Left = 0,
   Right = 1,
   Middle = 2
}

public class InputEvent {
    public static List<InputEvent> allEvents = new List<InputEvent>();
    public static Dictionary<KeyCode, InputEvent> keyEvents = new Dictionary<KeyCode, InputEvent>();
    public static Dictionary<MouseButton, InputEvent> mouseEvents = new Dictionary<MouseButton, InputEvent>();
    public static InputEvent<int> Numeric = new InputEvent<int>();

    public static InputEvent For(KeyCode keyCode) {
        if (!keyEvents.ContainsKey(keyCode)) {
            var ev = new InputEvent(keyCode);
            keyEvents[keyCode] = ev;
            allEvents.Add(ev);
        }

        return keyEvents[keyCode];
    }

    public static InputEvent For(MouseButton mouseButton) {
        if (!mouseEvents.ContainsKey(mouseButton)) {
            var ev = new InputEvent(mouseButton);
            mouseEvents[mouseButton] = ev;
            allEvents.Add(ev);
        }

        return mouseEvents[mouseButton];
    }

    public void Bind(MonoBehaviour comp, Action callback, bool repeat = false) {
        listeners.Add(new InputListener(comp, callback, repeat));
    }

    public static void Update() {
        foreach (var ev in keyEvents.Values.ToList()) {
            if (Input.GetKeyDown(ev.keyCode))
                ev.Trigger(repeat: false);
            else if (Input.GetKey(ev.keyCode))
                ev.Trigger(repeat: true);
        }

        foreach (var ev in mouseEvents.Values.ToList()) {
            if (Input.GetMouseButtonDown((int)ev.mouseButton))
                ev.Trigger(repeat: false);
            else if (Input.GetMouseButton((int)ev.mouseButton))
                ev.Trigger(repeat: true);
        }

        var i = Util.GetNumericKeyDown();
        if (i != -1) Numeric.Trigger(i);

        /*if (Input.GetKeyDown(KeyCode.P)) {
            if (Game.isPaused)
                Game.Unpause();
            else
                Game.Pause();
        }*/
    }

    string name;
    protected List<InputListener> listeners = new List<InputListener>();
    KeyCode keyCode;
    MouseButton mouseButton;

    public InputEvent() {
    }

    public InputEvent(KeyCode keyCode) {
        this.keyCode = keyCode;
    }

    public InputEvent(MouseButton mouseButton) {
        this.mouseButton = mouseButton;
    }

    public void Trigger(bool repeat = false) {
        for (var i = listeners.Count-1; i >= 0; i--) {
            var listener = listeners[i];
            if (listener.repeat == repeat && listener.comp.gameObject.activeInHierarchy) {
                listener.callback.Invoke();
                break;
            } 
        }
        
        CleanDestroyed();
    }

    public void CleanDestroyed() {
        var toKeep = new List<InputListener>();
        foreach (var listener in listeners) {
            if (listener.comp.gameObject != null)
                toKeep.Add(listener);
        }

        listeners = toKeep;
    }
}

public class InputEvent<T> : InputEvent {   
    public void Bind(MonoBehaviour comp, Action<T> callback, bool repeat = false) {
        listeners.Add(new InputListener<T>(comp, callback, repeat));
    }

    public void Trigger(T arg, bool repeat = false) {
        for (var i = listeners.Count-1; i >= 0; i--) {
            var listener = (InputListener<T>)listeners[i];
            if (listener.repeat == repeat && listener.comp.gameObject.activeInHierarchy) {
                listener.callback.Invoke(arg);
                break;
            } 
        }

        
        CleanDestroyed();
    }
}