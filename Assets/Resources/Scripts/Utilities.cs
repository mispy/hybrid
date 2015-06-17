using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PoolBehaviour : MonoBehaviour {
	public virtual void OnCreate() { }
	public virtual void OnRecycle() { }
}

public enum Orientation {
	up = 1,
	down = -1,
	left = 2,
	right = -2
}

public struct IntVector2 {
	public int x;
	public int y;

	public static double Distance(IntVector2 v1, IntVector2 v2) {
		return Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.y - v2.y, 2));
	}


	public static IntVector2 operator -(IntVector2 v1, IntVector2 v2) {
		return new IntVector2(v1.x - v2.x, v1.y - v2.y);
	}

	public static bool operator ==(IntVector2 v1, IntVector2 v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}

	public static bool operator !=(IntVector2 v1, IntVector2 v2) {
		return v1.x != v2.x || v1.y != v2.y;
	}

	public override string ToString()
	{
		return String.Format("IntVector2<{0}, {1}>", x, y);
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode();
	}
	
	public IntVector2(int x, int y) {
		this.x = x;
		this.y = y;
	}
}

public class Util {
	public static RaycastHit[] ParticleCast(ParticleSystem ps) {
		var radius = 0.05f;
		var length = ps.startSpeed * ps.startLifetime;
		return Physics.SphereCastAll(ps.transform.position, radius, ps.transform.up, length);
	}

	public static bool TurretBlocked(Ship ship, Vector3 turretPos, Vector3 targetPos) {		
		var targetDist = (targetPos - turretPos);
		var targetDir = targetDist.normalized;
		
		var targetHits = Physics.RaycastAll(turretPos, targetDir, targetDist.magnitude);
		foreach (var hit in targetHits) {
			if (hit.rigidbody == ship.rigidBody) {
				return true;
			}
		}

		return false;
	}

	public static bool TurretBlocked(Ship ship, Vector3 turretPos, Vector3 targetPos, float radius) {		
		var targetDist = (targetPos - turretPos);
		var targetDir = targetDist.normalized;
		
		var targetHits = Physics.SphereCastAll(turretPos, radius, targetDir, targetDist.magnitude, LayerMask.GetMask(new string[] { "Block", "Floor" }));
		foreach (var hit in targetHits) {
			if (ship.WorldToBlockPos(hit.collider.transform.position) != ship.WorldToBlockPos(turretPos)) {
				Debug.Log(ship.WorldToBlockPos(hit.collider.transform.position));
				return true;
			}
		}
		
		return false;
	}

	public static Vector2[] cardinals = new Vector2[] { Vector2.up, -Vector2.up, Vector2.right, -Vector2.right };

	public static Dictionary<Vector2, Orientation> cardinalToOrient = new Dictionary<Vector2, Orientation>() {
		{ Vector2.up, Orientation.up },
		{ -Vector2.up, Orientation.down },
		{ Vector2.right, Orientation.right },
		{ -Vector2.right, Orientation.left }
	};

	public static Dictionary<Orientation, Vector2> orientToCardinal = new Dictionary<Orientation, Vector2>() {
		{ Orientation.up, Vector2.up },
		{ Orientation.down, -Vector2.up },
		{ Orientation.right, Vector2.right },
		{ Orientation.left, -Vector2.right }
	};

	public static Vector2 Cardinalize(Vector2 vec) {
		var normal = vec.normalized;
		return cardinals.OrderBy((c) => Vector2.Distance(c, normal)).First();
	}
}