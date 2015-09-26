using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JumpMap : MonoBehaviour {        
    float jumpRange = 5f;
    Color hoverColor = new Color (0.95f, 0.64f, 0.38f, 0.05f);
    Color currentColor = new Color (0f, 1f, 1f, 1f);

    LineRenderer lineRenderer;
    List<JumpBeacon> beacons = new List<JumpBeacon>();
    List<JumpShip> jumpShips = new List<JumpShip>();
    JumpBeacon selectedBeacon;
    Canvas canvas;
    GameObject selector;

    Button enterButton;
    Button foldButton;
    Button waitButton;
    Button stopWaitButton;

    bool isWaiting = false;
    bool isJumping = false;

    public static void Activate() {
        Game.UnloadSector();
        Game.mainCamera = Game.jumpMap.GetComponentsInChildren<Camera>(includeInactive: true).First();
        Game.jumpMap.gameObject.SetActive(true);
    }

    public Vector3 GalaxyToWorldPos(Vector2 galaxyPos) {
        return new Vector3(galaxyPos.x/4, galaxyPos.y/4, 0);
    }

    void Awake() {
        canvas = GetComponentInChildren<Canvas>();
        selector = Pool.For("Selector").TakeObject();
        selector.transform.parent = transform;
        selector.SetActive(true);

        Game.mainCamera.orthographicSize = 4;
        var bounds = Util.GetCameraBounds(Game.mainCamera);

        foreach (var sector in SectorManager.all) {
            var beacon = Pool.For("JumpBeacon").Take<JumpBeacon>();
            beacon.sector = sector;
            sector.jumpBeacon = beacon;
            beacon.transform.parent = transform;
            beacon.transform.position = GalaxyToWorldPos(sector.galaxyPos);
            beacon.gameObject.SetActive(true);
            beacons.Add(beacon);
        }

        foreach (var ship in ShipManager.all) {
            var jumpShip = Pool.For("JumpShip").Take<JumpShip>();
            jumpShip.transform.parent = transform;
            jumpShip.Initialize(ship);
            jumpShip.gameObject.SetActive(true);
            jumpShips.Add(jumpShip);

            if (ship == Game.playerShip) {
                jumpShip.name = "JumpShip (Player)";
            }
        }

        SelectBeacon(Game.playerShip.sector.jumpBeacon);

        var positions = new List<Vector3>();

        DrawFactions();

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

    void EnterSector() {
        Game.LoadSector(Game.playerShip.sector);
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

    void FoldJump(JumpBeacon beacon) {
        Game.playerShip.FoldJump(beacon.sector);
    }

    void SelectBeacon(JumpBeacon beacon) {
        selectedBeacon = beacon;
        selector.transform.parent = beacon.transform.parent;
        selector.transform.position = beacon.transform.position;
    }

    void DrawFactions() {
        var beacon = beacons[0];
        var circle = Pool.For("FactionCircle").TakeObject();
        circle.transform.parent = beacon.transform;
        circle.transform.localPosition = Vector3.zero;
        circle.GetComponent<SpriteRenderer>().color = Color.green;
        circle.SetActive(true);
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
        
        if (selectedBeacon != null && selectedBeacon.sector == Game.playerShip.sector) {
            foldButton.gameObject.SetActive(false);
            enterButton.gameObject.SetActive(true);
        } else {
            foldButton.gameObject.SetActive(true);
            enterButton.gameObject.SetActive(false);
        }

        if (isWaiting || Game.playerShip.destSector != null)
            Game.galaxy.Simulate(Time.deltaTime);

        Game.MoveCamera(GalaxyToWorldPos(Game.playerShip.galaxyPos));
    }
}
