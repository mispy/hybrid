using UnityEngine;
using System.Collections;

public class OffsetScroller : MonoBehaviour {
	
	public float scrollSpeed;
	private Vector2 savedOffset;
	private Quaternion startRotation;
	private MeshRenderer renderer;

	void Start () {
		renderer = GetComponent<MeshRenderer>();
		savedOffset = renderer.sharedMaterial.GetTextureOffset ("_MainTex");
		startRotation = transform.rotation;
	}
	
	void Update () {
		Vector2 offset = Crew.player.transform.position / 100f;
		renderer.sharedMaterial.SetTextureOffset ("_MainTex", offset);
	}
	
	void OnDisable () {
		renderer.sharedMaterial.SetTextureOffset ("_MainTex", savedOffset);
	}
}