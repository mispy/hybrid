using UnityEngine;
using System.Collections;

public class JumpShip : PoolBehaviour {
	public BlockMap blocks;
	public JumpBeacon currentBeacon;

	public void FoldJump(JumpBeacon beacon) {
		StartCoroutine(MoveFunction(beacon));
	}

	IEnumerator MoveFunction(JumpBeacon beacon)
	{
		float timeSinceStarted = 0f;
		while (true)
		{
			timeSinceStarted += Time.deltaTime;
			transform.position = Vector3.Lerp(transform.position, beacon.transform.position, timeSinceStarted);
			
			// If the object has arrived, stop the coroutine
			if (Vector3.Distance(transform.position, beacon.transform.position) < Vector3.kEpsilon)
			{
				yield break;
			}

			// Otherwise, continue next frame
			yield return null;
		}
	}

	public void EnterSector() {
	}

	public void Rescale() {
		blocks = GetComponentInChildren<BlockMap>();
		transform.localScale = new Vector3(1.0f/blocks.Bounds.size.x, 1.0f/blocks.Bounds.size.y, 1.0f);
	}
}
