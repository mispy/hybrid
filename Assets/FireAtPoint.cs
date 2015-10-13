using UnityEngine;
using System.Collections;
using System.Linq;

public class FireAtPoint : BlockAbility {
    GameObject targetCircle;
    RotatingTurret[] turrets;
    
    public override bool WorksWith(Block block) {
        return block.type.GetComponent<RotatingTurret>() != null;
    }
    
    void Awake() {
        targetCircle = Pool.For("SetTargetCircle").TakeObject();
    }

    void OnEnable() {
        //targetCircle.SetActive(true);
        turrets = blocks.Select((b) => b.gameObject.GetComponent<RotatingTurret>()).ToArray();
        foreach (var turret in turrets) {
            turret.showLine = true;
        }
        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
    }

    void OnDisable() {
        foreach (var turret in turrets) {
            turret.dottedLine.enabled = false;
            turret.showLine = false;
        }
        targetCircle.SetActive(false);
    }

    void OnLeftClick() {

    }
    
    void FixedUpdate() {
        foreach (var turret in turrets) {
            turret.AimTowards(Game.mousePos);   
        }
                
        if (Input.GetMouseButton(0)) {
            foreach (var turret in turrets) {
                turret.gameObject.SendMessage("OnFire");
            }
        }
    }

}
