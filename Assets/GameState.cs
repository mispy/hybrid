﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

public static class SpaceLayer {
    public static LayerMask ShipBounds = LayerMask.GetMask(new string[] { "Bounds" });
}

public static class Game {
    public static GameState state;

    // cached main camera
    // Game.mainCamera seems to perform some kind of expensive lookup
    public static Camera mainCamera;

    public static Vector2 mousePos;
    public static ActiveSector activeSector;
    public static ShipControl shipControl;
    public static AbilityMenu abilityMenu;
    public static ShipDesigner shipDesigner;
    public static GameObject leaveSectorMenu;
    public static WeaponSelect weaponSelect;
    public static DebugMenu debugMenu;
    public static BlockSelector blockSelector;
    public static Player localPlayer;
    public static SpaceNetwork networkManager;

    public static Blockform playerShip {
        get { return Game.state.playerShip; }
        set { Game.state.playerShip = value; }
    }       

    public static bool isPaused {
        get { return Time.timeScale == 0.0f; }
    }

	public static bool debugVisibility = false;

    public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    static Game() {

        //GameObject.Find("Game").GetComponent<GameState>().UpdateRefs();
    }
    
    public static GameObject Prefab(string name) {
        if (!prefabs.ContainsKey(name)) {
            Debug.LogFormat("No prefab found for {0}. Available prefabs are: {1}", name, String.Join(", ", prefabs.Keys.ToArray()));
        }
        
        return prefabs[name];    
    }
    
    public static Sprite Sprite(string name) {
        if (!sprites.ContainsKey(name)) {
            Debug.LogFormat("No prefab found for {0}. Available prefabs are: {1}", name, String.Join(", ", sprites.Keys.ToArray()));
        }
        
        return sprites[name];
    }


    public static IEnumerable<T> LoadPrefabs<T>(string path) {
        foreach (var prefab in LoadPrefabs(path)) {
            var comp = prefab.GetComponent<T>();
            if (comp != null) yield return comp;
        }
    }
    
    public static IEnumerable<GameObject> LoadPrefabs(string path) {
        var resources = Resources.LoadAll(path);
        foreach (var obj in resources) {
            var gobj = obj as GameObject;
            if (gobj != null) {
                gobj.SendMessage("OnResourceLoad", SendMessageOptions.DontRequireReceiver);
                                
                yield return gobj;
            }
        }
    }
    
    
    public static IEnumerable<Texture2D> LoadTextures(string path) {
        var resources = Resources.LoadAll(path);
        foreach (var obj in resources) {
            var tex = obj as Texture2D;
            if (tex != null) {
                yield return tex;
            }
        }
    }

    
    public static void MoveCamera(Vector2 targetPos) {
        var pos = new Vector3(targetPos.x, targetPos.y, Game.mainCamera.transform.position.z);
        //Game.mousePos += (Vector2)(pos - Game.mainCamera.transform.position);
        Game.mainCamera.transform.position = pos;
    }
    
    public static void Pause() {
        Time.timeScale = 0.0f;
    }
    
    public static void Unpause() {
        Time.timeScale = 1.0f;
    }

    public static void Setup() {
        foreach (var prefab in Game.LoadPrefabs("Prefabs")) {
            if (prefab.GetComponent<NetworkIdentity>() != null) {
                if (Game.networkManager.spawnPrefabs != null)
                    Game.networkManager.spawnPrefabs.Add(prefab);
            }
            
            prefabs[prefab.name] = prefab;
        }
        
        foreach (var prefab in Game.LoadPrefabs("Beacons")) {
            prefabs[prefab.name] = prefab;
        }
        
        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites")) {
            sprites[sprite.name] = sprite;
        }                

        BlockType.LoadTypes();
    }

    public static void Start() {
        Game.state.gameObject.SetActive(true);
        Game.playerShip = Blockform.FromTemplate(Game.state.playerShipTemplate);
    }
}

public class GameState : MonoBehaviour {       
    public Canvas canvas;
    public Text debugText;
    public ShipTemplate2 playerShipTemplate;
    public Blockform playerShip;

    public void UpdateRefs() {
        Game.activeSector = GetComponentsInChildren<ActiveSector>(includeInactive: true).First();
        Game.shipControl = GetComponentsInChildren<ShipControl>(includeInactive: true).First();
        Game.abilityMenu = GetComponentsInChildren<AbilityMenu>(includeInactive: true).First();
        Game.shipDesigner = GetComponentsInChildren<ShipDesigner>(includeInactive: true).First();
        Game.leaveSectorMenu = GameObject.Find("LeavingSector");
        Game.weaponSelect = GetComponentInChildren<WeaponSelect>();
        Game.debugMenu = GetComponentsInChildren<DebugMenu>(includeInactive: true).First();
        Game.blockSelector = GetComponentsInChildren<BlockSelector>(includeInactive: true).First();
        Game.networkManager = GetComponentsInChildren<SpaceNetwork>(includeInactive: true).First();
        Game.mainCamera = Camera.main;
        Game.state = this;
    }
      
    public void Awake() {
        UpdateRefs();
        Game.Setup();
    }

    public void BriefMessage(string message) {
        messageText.text = message;
        Invoke("ClearMessage", 2.0f);
    }

    public void ClearMessage() {
        messageText.text = "";
    }

    public Text messageText;

    // Update is called once per frame
    void Update() {
        Game.mousePos = Game.mainCamera.ScreenToWorldPoint(Input.mousePosition); 

        InputEvent.Update();

        if (Input.GetKeyDown(KeyCode.Backslash)) {
            if (Game.debugMenu.gameObject.activeInHierarchy)
                Game.debugMenu.gameObject.SetActive(false);
            else
                Game.debugMenu.gameObject.SetActive(true);
        }
    }
}
