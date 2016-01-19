using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SpaceLayer {
    public static LayerMask ShipBounds = LayerMask.GetMask(new string[] { "Bounds" });
}

public class BlockInventory : ISaveable {
    Dictionary<BlockType, int> amounts = new Dictionary<BlockType, int>();

    void ISaveable.OnSave(SaveWriter save) {
        save.Write(amounts.Count);
        foreach (var item in amounts) {
            save.Write(item.Key);
            save.Write(item.Value);
        }
    }

    void ISaveable.OnLoad(SaveReader save) {
        var total = save.ReadInt32();
        for (var i = 0; i < total; i++) {
            amounts[save.ReadBlockType()] = save.ReadInt32();
        }
    }

    public int this[BlockType type] {
        get {
            if (!amounts.ContainsKey(type))
                return 0;
            return amounts[type];   
        }

        set {
            amounts[type] = Mathf.Clamp(value, 0, 1000);
        }
    }        
}

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class Game {
    public static GameState state;

    // cached main camera
    // Game.mainCamera seems to perform some kind of expensive lookup
    public static Camera mainCamera;

    public static Vector2 mousePos;
    public static ActiveSector activeSector;
    public static ShipControl shipControl;
    public static CrewControl crewControl;
    public static AbilityMenu abilityMenu;
    public static ShipDesigner shipDesigner;
    public static GameObject leaveSectorMenu;
    public static WeaponSelect weaponSelect;
    public static DebugMenu debugMenu;
    public static BlockSelector blockSelector;
    public static Player localPlayer;
    public static CameraControl cameraControl;
    public static FadeOverlay fadeOverlay;
    public static JumpMap jumpMap;
    public static MissionComplete missionComplete;
    public static BlockInventory inventory = new BlockInventory();
    public static HashSet<CrewBody> players = new HashSet<CrewBody>();
    public static Transform tmpHolder;
    public static SpaceNetwork network;

    public static bool debugMode {
        get {
            return Game.debugMenu.isActiveAndEnabled;
        }
    }

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
        foreach (var prefab in Game.LoadPrefabs("Prefabs")) {
            prefabs[prefab.name] = prefab;
        }

        foreach (var prefab in Game.LoadPrefabs("Beacons")) {
            prefabs[prefab.name] = prefab;
        }
        
        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites")) {
            sprites[sprite.name] = sprite;
        }                

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

    public static List<GameObject> prefabSequence = new List<GameObject>();

    public static int GetPrefabIndex(GameObject prefab) {
        var index = prefabSequence.IndexOf(prefab);
        if (index == -1) {
            throw new ArgumentException(String.Format("Unknown prefab {0}", prefab));
        }

        return index;
    }

    public static GameObject PrefabFromIndex(int index) {
        return prefabSequence[index];
    }

    public static IEnumerable<T> LoadPrefabs<T>(string path) {
        foreach (var prefab in LoadPrefabs(path)) {
            prefabSequence.Add(prefab);
            var comp = prefab.GetComponent<T>();
            if (comp != null) yield return comp;
        }
    }
    
    public static IEnumerable<GameObject> LoadPrefabs(string path) {
        var resources = Resources.LoadAll(path);
        foreach (var obj in resources) {
            var gobj = obj as GameObject;
            if (gobj != null) {
                prefabSequence.Add(gobj);
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

    public static void Save() {
        var path = Application.dataPath + "/Saves/player.ship";

        using (var stream = new FileStream(path, FileMode.Create)) {
            var save = new SaveWriter(stream);
            save.Write(Game.playerShip);
            save.Write(Game.inventory);
        }
    }
        
    public static void Load() {
        var path = Application.dataPath + "/Saves/player.ship";

        using (var stream = new FileStream(path, FileMode.Open)) {
            var save = new SaveReader(stream);
            Game.playerShip = Pool.For("Blockform").Attach<Blockform>(Game.tmpHolder);
            save.ReadSaveable(Game.playerShip);
            save.ReadSaveable(Game.inventory);
        }

        Game.activeSector.Load();
    }

    public static void New() {
        Game.playerShip = Blockform.FromTemplate(Game.state.playerShipTemplate);
        Game.activeSector.Load();
    }

    public static void Start() {
        SpaceNetwork.manager.GetComponent<NetworkManagerHUD>().enabled = false;
        Game.state.gameObject.SetActive(true);

        Game.Load();
    }
}

public class GameState : MonoBehaviour {       
    public Canvas canvas;
    public Text debugText;
    public ShipTemplate2 playerShipTemplate;
    public Blockform playerShip;
    public Transform tmpHolder;

    public T GetSingleton<T>() {
        // for brevity
        return GetComponentsInChildren<T>(includeInactive: false).First();
    }

    public void UpdateRefs() {
        Game.activeSector = GetComponentsInChildren<ActiveSector>(includeInactive: true).First();
        Game.shipControl = GetComponentsInChildren<ShipControl>(includeInactive: true).First();
        Game.crewControl = GetSingleton<CrewControl>();
        Game.abilityMenu = GetComponentsInChildren<AbilityMenu>(includeInactive: true).First();
        Game.shipDesigner = GetComponentsInChildren<ShipDesigner>(includeInactive: true).First();
        Game.leaveSectorMenu = GameObject.Find("LeavingSector");
        Game.weaponSelect = GetComponentInChildren<WeaponSelect>();
        Game.debugMenu = GetComponentsInChildren<DebugMenu>(includeInactive: true).First();
        Game.blockSelector = GetComponentsInChildren<BlockSelector>(includeInactive: true).First();
        Game.mainCamera = Camera.main;
        Game.cameraControl = GetComponentsInChildren<CameraControl>(includeInactive: true).First();
        Game.fadeOverlay = GetComponentsInChildren<FadeOverlay>(includeInactive: true).First();
        Game.jumpMap = GetComponentsInChildren<JumpMap>(includeInactive: true).First();
        Game.missionComplete = GetComponentsInChildren<MissionComplete>(includeInactive: true).First();
        Game.state = this;
        Game.tmpHolder = Game.state.tmpHolder;
    }
      
    public void Awake() {
        UpdateRefs();

        Game.state.gameObject.SetActive(false);
    }

    public void OnEnable() {
        UpdateRefs();
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
    }
}
