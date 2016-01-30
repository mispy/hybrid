using UnityEngine;
using System.Collections;

public class MissileLauncher : BlockComponent {
    CooldownCharger charger;

    void Awake() {
        charger = GetComponent<CooldownCharger>();
    }

    public void Fire(Block targetBlock) {
        charger.Discharge();
        var missile = Pool.For("Torpedo").Attach<GuidedMissile>(Game.activeSector.transients);
        missile.GetComponent<Explosive>().originComp = this;
        missile.targetBlock = targetBlock;
        missile.transform.position = Util.TipPosition(block);
        missile.transform.rotation = transform.rotation;
    }
}
