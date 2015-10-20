using UnityEngine;
using System.Collections;

public class FoldMapCamera : MonoBehaviour {
    new Camera camera;

	void Start () {
	    camera = GetComponent<Camera>();
	}
	
    public void ZoomIn() {  
        var newSize = (int)camera.orthographicSize >> 1;
        camera.orthographicSize = Mathf.Max(newSize, 4);
    }
    
    public void ZoomOut() {
        var newSize = (int)camera.orthographicSize << 1;
        camera.orthographicSize = Mathf.Min(newSize, 1 << 16);
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Equals)) {
            ZoomIn();
        } else if (Input.GetKeyDown(KeyCode.Minus)) {
            ZoomOut();
        }
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            ZoomIn();
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            ZoomOut();
        }
    }
}
