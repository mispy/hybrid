using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JumpMap : PoolBehaviour {        
    float jumpRange = 5f;
    Color hoverColor = new Color (0.95f, 0.64f, 0.38f, 0.05f);
    Color currentColor = new Color (0f, 1f, 1f, 1f);

    LineRenderer lineRenderer;
    List<Jumpable> beacons = new List<Jumpable>();
    List<JumpShip> jumpShips = new List<JumpShip>();
    Jumpable selectedBeacon;
    SectorInfo sectorInfo;
    Canvas canvas;
    Transform selector;

    Button enterButton;
    Button foldButton;
    Button waitButton;
    Button stopWaitButton;

    bool isWaiting = false;
    bool isJumping = false;
    Transform contents;


    public static void Activate() {
        Game.UnloadSector();
        Game.mainCamera = Game.jumpMap.GetComponentsInChildren<Camera>(includeInactive: true).First();
        Game.jumpMap.gameObject.SetActive(true);
    }

    public Vector3 GalaxyToWorldPos(GalaxyPos galaxyPos) {
        return new Vector3(galaxyPos.x, galaxyPos.y, 0);
    }

    void Awake() {
        canvas = GetComponentInChildren<Canvas>();
        sectorInfo = GetComponentInChildren<SectorInfo>();
        selector = Pool.For("Selector").Attach<Transform>(transform);
        contents = Pool.For("Holder").Attach<Transform>(transform);
        contents.name = "Contents";
        contents.gameObject.SetActive(true);

        foreach (var button in GetComponentsInChildren<Button>(includeInactive: true)) {
            if (button.name == "FoldButton") {
                foldButton = button;
                foldButton.onClick.AddListener(() => FoldJump(selectedBeacon));
            }
            
            if (button.name == "EnterButton") {
                enterButton = button;
                enterButton.onClick.AddListener(() => EnterSector());
            }
            
            if (button.name == "WaitButton") {
                waitButton = button;
                waitButton.onClick.AddListener(() => Wait());
            }
            
            if (button.name == "StopWaitButton") {
                stopWaitButton = button;
                stopWaitButton.onClick.AddListener(() => StopWait());
            }
        }
    }

    void OnEnable() {
        Game.mainCamera.orthographicSize = 4;
        //var bounds = Util.GetCameraBounds(Game.mainCamera);

        foreach (Transform child in contents)
            Pool.Recycle(child.gameObject);
        beacons.Clear();
        jumpShips.Clear();

        foreach (Star star in Game.galaxy.stars) {
            var beacon = Pool.For("Star").Attach<Jumpable>(contents);
            beacon.transform.position = star.transform.position;
            beacon.renderer.color = star.faction.color;
            beacons.Add(beacon);
        }

        foreach (var ship in Ship.all) {
            if (ship.isStationary) continue;

            var jumpShip = Pool.For("JumpShip").Attach<JumpShip>(contents);
            jumpShip.Initialize(ship);
            jumpShips.Add(jumpShip);

            if (ship == Game.playerShip) {
                jumpShip.name = "JumpShip (Player)";
            }
        }

        SelectBeacon(Game.playerShip.jumpPos);
        DrawFactions();
    }

    void EnterSector() {
        Game.LoadSector(Game.playerShip.jumpPos.GetComponent<Beacon>());
        gameObject.SetActive(false);
    }

    void Wait() {
        isWaiting = true;
        waitButton.gameObject.SetActive(false);
        stopWaitButton.gameObject.SetActive(true);
    }

    void StopWait() {
        isWaiting = false;
        waitButton.gameObject.SetActive(true);
        stopWaitButton.gameObject.SetActive(false);
    }

    void FoldJump(Jumpable dest) {
        Game.playerShip.FoldJump(dest);
    }

    void SelectBeacon(Jumpable beacon) {
        selectedBeacon = beacon;
        selector.transform.position = beacon.transform.position;
        sectorInfo.ShowInfo(selectedBeacon.sector);
    }

    void DrawFactions() {
        var beacon = beacons[0];
        /*var circle = Pool.For("FactionCircle").TakeObject();
        circle.transform.parent = beacon.transform;
        circle.transform.localPosition = Vector3.zero;
        circle.GetComponent<SpriteRenderer>().color = Color.green;
        circle.SetActive(true);*/
    }

    // Update is called once per frame
    void Update() {
        Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

        var nearMouseBeacon = beacons.OrderBy ((b) => Vector3.Distance (b.transform.position, pz)).First();
        
        if (Input.GetKeyDown(KeyCode.J)) {
            gameObject.SetActive(false);
        }

        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                SelectBeacon(nearMouseBeacon);
            }
        }
        
        if (selectedBeacon != null && selectedBeacon == Game.playerShip.jumpPos) {
            foldButton.gameObject.SetActive(false);
            enterButton.gameObject.SetActive(true);
        } else {
            foldButton.gameObject.SetActive(true);
            enterButton.gameObject.SetActive(false);
        }

        if (isWaiting || Game.playerShip.jumpDest != null)
            Game.galaxy.Simulate(Time.deltaTime);

        Game.MoveCamera(GalaxyToWorldPos(Game.playerShip.galaxyPos));
    }
}
