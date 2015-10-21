using UnityEngine;
using System.Collections;
using System.Linq;

public class SetTarget : BlockAbility {
    Transform targetCircle;
    RotatingTurret[] turrets;

    /*public override bool WorksWith(Block block) {
        return block.type.GetComponent<RotatingTurret>() != null;
    }*/

    void OnEnable() {
        targetCircle = Pool.For("SetTargetCircle").Attach<Transform>(transform);
        turrets = blocks.Select((b) => b.gameObject.GetComponent<RotatingTurret>()).ToArray();

        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
    }

    public void OnLeftClick() {    
        Target(Blockform.AtWorldPos(Game.mousePos), Game.mousePos);
        gameObject.SetActive(false);
    }

    void OnDisable() {
        if (targetCircle != null)
            Pool.Recycle(targetCircle.gameObject);
    }

    void Target(Blockform form, Vector2 pos) {
        targetCircle.transform.SetParent(form.transform);
        targetCircle = null;
    }

    void Update() {
        var form = Blockform.AtWorldPos(Game.mousePos);
        if (form == null) return;

        targetCircle.transform.position = Game.mousePos;

        /*foreach (var turret in turrets) {
            turret.AimTowards(Game.mousePos);   
        }*/
	}
}
