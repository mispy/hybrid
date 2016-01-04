using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveSector : MonoBehaviour {
    private Transform _contents;
    public Transform contents {
        get {
            if (_contents == null) {
                _contents = Pool.For("Holder").Attach<Transform>(transform);
                _contents.name = "Contents";
            }

            return _contents;
        }
    }

    private Transform _transients;
    public Transform transients {
        get {
            if (_transients == null) {
                _transients = Pool.For("Holder").Attach<Transform>(transform);
                _transients.name = "Transients";
            }
            
            return _transients;
        }
    }

    public float radius {
        get {
            return 50f;
        }
    }

    public List<Blockform> blockforms;

    public bool IsOutsideBounds(Vector3 pos) {
        return pos.magnitude > 50f;
    }

    public void OnEnable() {
    }
}
