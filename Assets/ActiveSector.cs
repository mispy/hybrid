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

    public List<Blockform> blockforms;

    public void OnEnable() {
        Game.mainCamera = GetComponentInChildren<Camera>();
    }

    public void RealizeShip(Ship ship, Vector2 pos) {
        var form = ship.LoadBlockform();
        form.transform.SetParent(contents);
		form.transform.position = pos;
        form.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector2.zero - pos);
        form.gameObject.SetActive(true);
    }

    public void RealizeShip(Ship ship) {
        RealizeShip(ship, new Vector2(0, 0));
    }
}
