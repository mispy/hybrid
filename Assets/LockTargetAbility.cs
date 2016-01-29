using UnityEngine;
using System.Collections;
using System.Linq;

public class LockTargetAbility : BlockAbility {
    Block targetBlock;
    MissileLauncher[] launchers;
    float startTargetScale = 2f;
    float endTargetScale = 1f;
    float targetTime = 1f;
    float targetCounter = 1f;


    public override bool WorksWith(Block block) {
        return block.type.GetComponent<MissileLauncher>() != null;
    }

    private SpriteRenderer _targetIndicator;
    public SpriteRenderer targetIndicator {
        get {
            if (_targetIndicator == null) {                
                _targetIndicator = Pool.For("Selector").Attach<SpriteRenderer>(transform);
                _targetIndicator.color = Color.red;
                _targetIndicator.enabled = false;
            }
            return _targetIndicator;
        }
    }

    void OnEnable() {
        launchers = blocks.Select((b) => b.gameObject.GetComponent<MissileLauncher>()).ToArray();
        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
    }

    void OnDisable() {
        targetIndicator.enabled = false;
    }

    void OnLeftClick() {
        var block = Block.AtWorldPos(Game.mousePos).FirstOrDefault();
        if (block == null) return;

        targetBlock = block;
        targetIndicator.transform.SetParent(targetBlock.ship.transform);
        targetIndicator.transform.position = targetBlock.worldPos;
        targetIndicator.transform.rotation = targetBlock.ship.transform.rotation;
        targetIndicator.enabled = true;
        targetCounter = 0f;
    }

    void Update() {
/*        if (!Input.GetMouseButtonDown(0)) {
            targetBlock = null;
            targetIndicator.enabled = false;
            return;
        }*/
        if (targetBlock == null) return;

        targetCounter += Time.deltaTime;
        targetIndicator.transform.localScale = Vector2.one*Mathf.Lerp(startTargetScale, endTargetScale, targetCounter/targetTime);
    }
}
