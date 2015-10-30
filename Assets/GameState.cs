using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;
using UnityEditor;

public static class SpaceLayer {
    public static LayerMask ShipBounds = LayerMask.GetMask(new string[] { "Bounds" });
}

[InitializeOnLoad]
public static class Game {
    public static GameState state;

    // cached main camera
    // Game.mainCamera seems to perform some kind of expensive lookup
    public static Camera mainCamera;

    // cached mouse position in world coordinates
    public static Galaxy galaxy;

    public static Vector2 mousePos;
    public static SectorKind activeSector;
    public static JumpMap jumpMap;
    public static ShipControl shipControl;
    public static AbilityMenu abilityMenu;
    public static DialogueMenu dialogueMenu;
    public static ShipDesigner shipDesigner;
    public static GameObject leaveSectorMenu;
    public static WeaponSelect weaponSelect;
    public static ShipInfo shipInfo;
    public static DebugMenu debugMenu;

    public static Ship playerShip {
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
        foreach (var prefab in Game.LoadPrefabs("Prefabs")) {
            prefabs[prefab.name] = prefab;
        }

        foreach (var prefab in Game.LoadPrefabs("Beacons")) {
            prefabs[prefab.name] = prefab;
        }
        
        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites")) {
            sprites[sprite.name] = sprite;
        }                

        GameObject.Find("Game").GetComponent<GameState>().UpdateRefs();
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
        var resources = Resources.LoadAll(path);

        foreach (var obj in resources) {
            var gobj = obj as GameObject;
            if (gobj != null) {
                var comp = gobj.GetComponent<T>();
                //Debug.LogFormat("{0} {1}", "Loaded", gobj);

                if (comp != null) yield return comp;
            }
        }
    }
    
    public static IEnumerable<GameObject> LoadPrefabs(string path) {
        var resources = Resources.LoadAll(path);
        foreach (var obj in resources) {
            var gobj = obj as GameObject;
            if (gobj != null) {
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

    public static void LoadSector(SectorKind sector) {
    }
    
    public static void UnloadSector() {
        foreach (var form in Game.activeSector.blockforms.ToList()) {
            form.ship.UnloadBlockform();
        }

        foreach (Transform child in Game.activeSector.contents) {
            Pool.Recycle(child.gameObject);
        }
        
        Game.activeSector.gameObject.SetActive(false);
    }
}

public class GameState : MonoBehaviour {       
    public Canvas canvas;
    public Text debugText;
    public ShipTemplate2 playerShipTemplate;
    public Ship playerShip;

    public void UpdateRefs() {
        Game.galaxy = GetComponentsInChildren<Galaxy>(includeInactive: true).First();
        Game.activeSector = GetComponentsInChildren<ActiveSector>(includeInactive: true).First();
        Game.jumpMap = GetComponentsInChildren<JumpMap>(includeInactive: true).First();
        Game.shipControl = GetComponentsInChildren<ShipControl>(includeInactive: true).First();
        Game.abilityMenu = GetComponentsInChildren<AbilityMenu>(includeInactive: true).First();
        Game.dialogueMenu = GetComponentsInChildren<DialogueMenu>(includeInactive: true).First();
        Game.shipDesigner = GetComponentsInChildren<ShipDesigner>(includeInactive: true).First();
        Game.leaveSectorMenu = GameObject.Find("LeavingSector");
        Game.weaponSelect = GetComponentInChildren<WeaponSelect>();
        Game.shipInfo = GetComponentInChildren<ShipInfo>();
        Game.debugMenu = GetComponentsInChildren<DebugMenu>(includeInactive: true).First();
        Game.mainCamera = Camera.main;
        Game.state = this;
    }

    public void Awake() {
        UpdateRefs();
    }

    public void OnEnable() {
        UpdateRefs();
    }

    public void Start() {
        Game.playerShip = Ship.FromTemplate(Game.state.playerShipTemplate);

        var jumpables = Util.Shuffle(Game.galaxy.GetComponentsInChildren<Jumpable>());
        foreach (var jump in jumpables) {
            if (jump.sectors.Any()) {
                Game.LoadSector(Util.GetRandom(jump.sectors));
                Game.activeSector.RealizeShip(Game.playerShip);
            }
        }
    }

    public void BriefMessage(string message) {
        messageText.text = message;
        Invoke("ClearMessage", 2.0f);
    }

    public void ClearMessage() {
        messageText.text = "";
    }

    public static bool inputBlocked = false;
    public static string inputBlocker;

    public static void BlockInput(string blocker) {
        inputBlocked = true;
        inputBlocker = blocker;
    }

    public static void UnblockInput() {
        inputBlocked = false;
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

        if (Input.GetKeyDown(KeyCode.F5)) {
            //Save.SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9)) {
            //Save.LoadGame();
        }
    }
}
