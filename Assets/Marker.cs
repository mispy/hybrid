using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Marker : MonoBehaviour {
    public string tagged;

    public bool Matches(ShipTemplate2 template) {
        return true;
    }

    public void Realize() {
        var matches = new List<ShipTemplate2>();

        foreach (var template in ShipTemplate2.All) {
            if (this.Matches(template)) {
                matches.Add(template);
            }
        }

        var match = Util.GetRandom(matches);
        match.Realize(transform.position);
        Pool.Recycle(this.gameObject);
    }
}
