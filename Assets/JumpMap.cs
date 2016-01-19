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
    public JumpBeacon selectedBeacon;
    Canvas canvas;
    Transform selector;

    Button enterButton;
    Button foldButton;
    Button waitButton;
    Button stopWaitButton;

    bool isWaiting = false;
    bool isJumping = false;

    void Awake() {
        canvas = GetComponentInChildren<Canvas>();
        selector = Pool.For("Selector").Attach<Transform>(transform);

        for (var i = 0; i < 10; i++) {
            var beacon = Pool.For("JumpBeacon").Attach<JumpBeacon>(transform);
            beacon.transform.position = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5));
            beacons.Add(beacon);
        }

        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);

/*        foreach (var button in GetComponentsInChildren<Button>(includeInactive: true)) {
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
        }*/

    }

    void OnEnable() {
        Game.mainCamera.orthographicSize = 8;
        Game.cameraControl.Lock(transform);
    }

    void JumpTo(JumpBeacon beacon) {
        Game.jumpMap.gameObject.SetActive(false);
        Game.activeSector.gameObject.SetActive(true);
        beacon.mission.Activate();
        var ship = Game.playerShip;
        var targetPosition = new Vector2(0, 50);
        var duration = 0.5f;
//        Game.cameraControl.Lock(Game.localPlayer.transform);
        Game.mainCamera.orthographicSize = 256;
        Game.cameraControl.Lock(Game.activeSector.contents);
        Game.MoveCamera(targetPosition);
        Game.activeSector.Load();

        ship.rigidBody.velocity = Vector2.up * 1000;
        Invoke("EndJump", duration);
        ship.transform.position = new Vector2(targetPosition.x, targetPosition.y - ship.rigidBody.velocity.y*duration);
    }

    void EndJump() {
        Game.playerShip.rigidBody.velocity = Vector2.zero;
        Game.cameraControl.Lock(Game.localPlayer.transform);
    }

    public void OnLeftClick() {
        JumpTo(selectedBeacon);
    }

    void HoverBeacon(JumpBeacon beacon) {
        selector.transform.position = beacon.transform.position;
        selectedBeacon = beacon;
    }

    // Update is called once per frame
    void Update() {
        var nearMouseBeacon = beacons.OrderBy ((b) => Vector3.Distance (b.transform.position, Game.mousePos)).First();

        if (Input.GetKeyDown(KeyCode.J)) {
            gameObject.SetActive(false);
        }

        if (!EventSystem.current.IsPointerOverGameObject()) {
            HoverBeacon(nearMouseBeacon);
        }

    }
}