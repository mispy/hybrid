using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

public static class SpaceLayer {
    public static LayerMask ShipBounds = LayerMask.GetMask(new string[] { "Bounds" });
}

public class Game : MonoBehaviour {
    public static Game main;

    // cached main camera
    // Game.mainCamera seems to perform some kind of expensive lookup
    public static Camera mainCamera;

    // cached mouse position in world coordinates
    public static Galaxy galaxy;
    public static Ship playerShip;

    public static Vector2 mousePos;
    public static ActiveSector activeSector;
    public static JumpMap jumpMap;
    public static ShipControl shipControl;
    public static AbilityMenu abilityMenu;
    public static DialogueMenu dialogueMenu;

    public static bool isPaused {
        get { return Time.timeScale == 0.0f; }
    }

	public static bool debugVisibility = false;

    public Canvas canvas;
    public Text debugText;

    public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

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

    public ShipDesigner shipDesigner;
    public WeaponSelect weaponSelect;
    public Text messageText;

    void MakeUniverse() {
		FactionManager.Create("Dragons");
		FactionManager.Create("Mushrooms");
		var mitzubi = FactionManager.Create("Mitzubi Navy", color: new Color(251/255.0f, 213/255.0f, 18/255.0f));
		FactionManager.Create("Cats");
        var pirateGang = FactionManager.Create("Pirate Gang", color: Color.red);

        pirateGang.opinion[mitzubi].Change(-1000, OpinionReason.AttackedMyShip);
        mitzubi.opinion[pirateGang].Change(-1000, OpinionReason.AttackedMyShip);

        for (var i= 0; i < 100; i++) {
            Star.Create();
        }

        foreach (var star in Star.all) {
            FactionOutpost.Create(star.BeaconPosition(), faction: mitzubi);
            ConflictZone.Create(star.BeaconPosition(), attacking: pirateGang, defending: mitzubi);
        }

		var sector = SectorManager.all[0];
		//ShipManager.Create(sector: sector, faction: FactionManager.all[1], sectorPos: new Vector2(100, 0));
		Game.playerShip = Ship.Create(sector: sector, faction: mitzubi, sectorPos: new Vector2(-100, 0));
    }
    
    public static void LoadSector(Sector sector) {
        sector.type.OnRealize();

        activeSector.sector = sector;

        foreach (var ship in sector.ships) {
            activeSector.RealizeShip(ship);
        }

        activeSector.gameObject.SetActive(true);
        Game.mainCamera = activeSector.GetComponentInChildren<Camera>();
    }
    
    public static void UnloadSector() {
        foreach (Transform child in Game.activeSector.contents) {
            Pool.Recycle(child.gameObject);
        }
        activeSector.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Awake () {        
        Game.galaxy = new Galaxy();
        Game.activeSector = GetComponentInChildren<ActiveSector>();
        Game.jumpMap = GetComponentsInChildren<JumpMap>(includeInactive: true).First();
        Game.shipControl = GetComponentInChildren<ShipControl>();
        Game.abilityMenu = GetComponentsInChildren<AbilityMenu>(includeInactive: true).First();
        Game.dialogueMenu = GetComponentsInChildren<DialogueMenu>(includeInactive: true).First();
        Game.main = this;


        foreach (var prefab in Game.LoadPrefabs("Prefabs")) {
            prefabs[prefab.name] = prefab;
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites")) {
            sprites[sprite.name] = sprite;
        }

        Tile.Setup();
        Block.Setup();
        Pool.CreatePools();        
        ShipTemplate.LoadAll();

        MakeUniverse();
        //Save.LoadGame();


        //Generate.EllipsoidShip(new Vector2(12, 0), 20, 10);

        //Generate.TestShip(new Vector2(5, 0));
        //for (var i = 0; i < 5; i++) {
        //    Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
        //}
        //InvokeRepeating("GenerateShip", 0.0f, 1.0f);

        for (var i = 0; i < 100; i++) {
            //debug.MakeAsteroid(new Vector2(Random.Range(-sectorSize, sectorSize), Random.Range(-sectorSize, sectorSize)));
        }
        Tests.Run();
    }

    void Start() {        
        Game.LoadSector(SectorManager.all[0]);
        Game.mainCamera = Camera.main;
    }

    public Text debugMenu;

    // Update is called once per frame
    void Update() {
        mousePos = Game.mainCamera.ScreenToWorldPoint(Input.mousePosition); 
             
        if (Game.inputBlocked) return;

        InputEvent.Update();

        if (Input.GetKeyDown(KeyCode.Backslash)) {
            if (debugMenu.gameObject.activeInHierarchy)
                debugMenu.gameObject.SetActive(false);
            else
                debugMenu.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.F5)) {
            //Save.SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9)) {
            //Save.LoadGame();
        }
    }
}
