using UnityEngine;

public class TestCamera : MonoBehaviour {
		Vector3 target = Vector2.zero;
		Vector3 pos;

		// animation settings
		public Vector3 minoffset = Vector3.one;
		public Vector3 maxoffset = Vector3.one;
		public Vector3 frequency = Vector3.one;
		public Vector3 smoothing = Vector3.one;
		public Vector3 life = Vector3.zero;
		public float min = 0.1f;
		public float max = 18f;
		public Vector3 manualSpeed = Vector3.one;

		float animTimer = 0f;

		bool doAnimation = true;
		float animDelay = 10f;

		// Use this for initialization
		void Start() {
			pos = Camera.main.transform.position;
			target = pos;
			target.z = Camera.main.orthographicSize;
		}

		// Update is called once per frame
		void Update() {
			if (Input.GetKey(KeyCode.LeftArrow))
				target.x -= manualSpeed.x;
			if (Input.GetKey(KeyCode.RightArrow))
			    target.x += manualSpeed.x;
			if (Input.GetKey(KeyCode.UpArrow))
				target.y += manualSpeed.y;
			if (Input.GetKey(KeyCode.DownArrow))
				target.y -= manualSpeed.y;
			if (Input.GetKey(KeyCode.A))
				target.z -= manualSpeed.z;
			if (Input.GetKey(KeyCode.Z))
				target.z += manualSpeed.z;

			//manual input cancels animation
			if (Input.anyKey) {
				doAnimation = false;
				animTimer = 0;	
			}

			//animation
			if (doAnimation) {
				//position
				target.z = minoffset.z + (0.5f + (Mathf.Sin (life.z) * 0.5f)) * (maxoffset.z - minoffset.z);
				target.x = minoffset.x + (0.5f + (Mathf.Sin (life.x) * 0.5f)) * (maxoffset.x - minoffset.x);
				target.y = minoffset.y + (0.5f + (Mathf.Sin (life.y) * 0.5f)) * (maxoffset.y - minoffset.y);
				life += frequency;
			} else {
				animTimer += Time.deltaTime;
				
				if(animTimer > animDelay)
					doAnimation = true;
			}

			//cap zoom
			target.z = target.z < min ? min : target.z;
			target.z = target.z > max ? max : target.z;

			//zoom in/out
			Camera.main.orthographicSize += (float)((int)((target.z - Camera.main.orthographicSize) * smoothing.z * 1000)) / 1000;
			
			//left/right
			pos.x += (float)((int)((target.x - GetComponent<Camera>().transform.position.x) * smoothing.x * 1000)) / 1000;
			pos.y += (float)((int)((target.y - GetComponent<Camera>().transform.position.y) * smoothing.y * 1000)) / 1000;

			transform.position = pos;
		}
}


