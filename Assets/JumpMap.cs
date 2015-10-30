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
    Jumpable[] jumpables;
    Jumpable selectedJump;
    SectorInfo sectorInfo;
    Canvas canvas;
    Transform selector;

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

    void Awake() {
        canvas = GetComponentInChildren<Canvas>();
        sectorInfo = GetComponentInChildren<SectorInfo>();
        selector = Pool.For("Selector").Attach<Transform>(transform);

        foreach (var button in GetComponentsInChildren<Button>(includeInactive: true)) {
            if (button.name == "FoldButton") {
                foldButton = button;
                foldButton.onClick.AddListener(() => FoldJump());
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
        Game.mainCamera.transform.position = Game.playerShip.transform.position;
        //var bounds = Util.GetCameraBounds(Game.mainCamera);

        jumpables = Game.galaxy.GetComponentsInChildren<Jumpable>();                

        SelectJumpable(Game.playerShip.jumpPos);
        DrawFactions();
    }

    void EnterSector() {
        //Game.LoadSector(Game.playerShip.jumpPos.GetComponent<Jumpable>());
        gameObject.SetActive(false);
    }

    void Wait() {
        isWaiting = true;
        Galaxy.timeScale = 0f;
        waitButton.gameObject.SetActive(false);
        stopWaitButton.gameObject.SetActive(true);
    }

    void StopWait() {
        isWaiting = false;
        Galaxy.timeScale = 1f;
        waitButton.gameObject.SetActive(true);
        stopWaitButton.gameObject.SetActive(false);
    }

    void FoldJump() {
        Game.playerShip.FoldJump(selectedJump);
    }

    void SelectJumpable(Jumpable jumpable) {
        selectedJump = jumpable;
        selector.transform.position = jumpable.transform.position;
    }

    void DrawFactions() {
        /*var circle = Pool.For("FactionCircle").TakeObject();
        circle.transform.parent = beacon.transform;
        circle.transform.localPosition = Vector3.zero;
        circle.GetComponent<SpriteRenderer>().color = Color.green;
        circle.SetActive(true);*/
    }

    // Update is called once per frame
    void Update() {
        Vector2 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

        var nearMouseJump = jumpables.OrderBy ((b) => Vector3.Distance (b.transform.position, pz)).First();
        
        if (Input.GetKeyDown(KeyCode.J)) {
            gameObject.SetActive(false);
        }

        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                SelectJumpable(nearMouseJump);
            }
        }
        
        if (selectedJump != null && selectedJump == Game.playerShip.jumpPos) {
            foldButton.gameObject.SetActive(false);
            enterButton.gameObject.SetActive(true);
        } else {
            foldButton.gameObject.SetActive(true);
            enterButton.gameObject.SetActive(false);
        }

        Game.MoveCamera(Game.playerShip.galaxyPos);
    }
}
