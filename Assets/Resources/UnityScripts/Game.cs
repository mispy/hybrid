using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
    public static Game main;

    // cached main camera
    // Game.mainCamera seems to perform some kind of expensive lookup
    public static Camera mainCamera;

    // cached mouse position in world coordinates
    public static Galaxy galaxy;
    public static Ship playerShip;

    public static Vector3 mousePos;
    public static ActiveSector activeSector;
    public static JumpMap jumpMap;
    public static ShipControl shipControl;

    public static bool isPaused {
        get { return Time.timeScale == 0.0f; }
    }

    public Canvas canvas;
    public Text debugText;

    public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    public static GameObject Prefab(string name) {
        if (!prefabs.ContainsKey(name)) {
            Debug.LogFormat("No prefab found for {0}. Available prefabs are: {1}", name, String.Join(", ", prefabs.Keys.ToArray()));
        }

        return prefabs[name];
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
        var cosmicWidth = 100;
        var cosmicHeight = 100;    
        
        FactionManager.Add(new Faction("Dragons"));
        FactionManager.Add(new Faction("Mushrooms"));
        FactionManager.Add(new Faction("Bees"));
        FactionManager.Add(new Faction("Tictacs"));

        for (var i = 0; i < 100; i++) {
            var nsec = new Sector();
            var x = Random.Range(-cosmicWidth/2, cosmicWidth/2);
            var y = Random.Range(-cosmicHeight/2, cosmicHeight/2);
            nsec.galaxyPos = new Vector2(x, y);
            SectorManager.Add(nsec);    
        }

		var sector = SectorManager.all[0];
		//ShipManager.Create(template: "Station", sector: sector, sectorPos: new Vector2(0, 0), faction: FactionManager.all[0]);
		ShipManager.Create(template: "New Frigate", sector: sector, faction: FactionManager.all[1], sectorPos: new Vector2(100, 0));
		Game.playerShip = ShipManager.Create(template: "New Frigate", sector: sector, faction: FactionManager.all[2], sectorPos: new Vector2(-100, 0));
    }
    
    public static void LoadSector(Sector sector) {
        activeSector.sector = sector;

        foreach (var ship in ShipManager.all) {
            if (ship.sector == sector) {
                activeSector.RealizeShip(ship);
            }
        }
        
        activeSector.gameObject.SetActive(true);
        Game.mainCamera = activeSector.GetComponentInChildren<Camera>();
    }
    
    public static void UnloadSector() {
        foreach (var obj in activeSector.GetComponentsInChildren<PoolBehaviour>())
            Pool.Recycle(obj.gameObject);
        
        activeSector.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Awake () {        
        Game.galaxy = new Galaxy();
        Game.activeSector = GetComponentInChildren<ActiveSector>();
        Game.jumpMap = GetComponentsInChildren<JumpMap>(includeInactive: true).First();
        Game.shipControl = GetComponentInChildren<ShipControl>();
        Game.main = this;

        var resources = Resources.LoadAll("Prefabs");
        
        foreach (var obj in resources) {
            var gobj = obj as GameObject;
            if (gobj != null) {
                prefabs[obj.name] = gobj;
            }
        }

        Tile.Setup();
        Block.Setup();
        Pool.CreatePools();        
        ShipManager.LoadTemplates();

        MakeUniverse();

        Game.LoadSector(SectorManager.all[0]);
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
    }

    void GenerateShip() {
        Generate.TestShip(new Vector2(Random.Range(-50, 50), Random.Range(-50, 50)));
    }

    void Start() {
        Game.mainCamera = Camera.main;
    }

    public Text debugMenu;

    // Update is called once per frame
    void Update() {
        mousePos = Game.mainCamera.ScreenToWorldPoint(Input.mousePosition); 
        mousePos.z = 0f;

        if (Game.inputBlocked) return;

        if (Input.GetKeyDown(KeyCode.BackQuote)) {
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
