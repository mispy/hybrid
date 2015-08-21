﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

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

public struct IntVector3 {
	public int x;
	public int y;
	public int z;

	public static double Distance(IntVector3 v1, IntVector3 v2) {
		return Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.y - v2.y, 2));
	}


	public static IntVector3 operator -(IntVector3 v1, IntVector3 v2) {
		return new IntVector3(v1.x - v2.x, v1.y - v2.y);
	}

	public static bool operator ==(IntVector3 v1, IntVector3 v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}

	public static bool operator !=(IntVector3 v1, IntVector3 v2) {
		return v1.x != v2.x || v1.y != v2.y;
	}

	public override string ToString()
	{
		return String.Format("IntVector3<{0}, {1}>", x, y);
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode();
	}
	
	public IntVector3(int x, int y) {
		this.x = x;
		this.y = y;
		this.z = 0;
	}

	public IntVector3(int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
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
		
		var targetHits = Physics.SphereCastAll(turretPos, radius, targetDir, targetDist.magnitude, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
		foreach (var hit in targetHits) {
			if (ship.WorldToBlockPos(hit.collider.transform.position) != ship.WorldToBlockPos(turretPos)) {
				Debug.Log(ship.WorldToBlockPos(hit.collider.transform.position));
				return true;
			}
		}
		
		return false;
	}

	public static bool LineOfSight(GameObject obj, Vector3 targetPos) {
		var targetDist = (targetPos - obj.transform.position);
		var targetDir = targetDist.normalized;
		
		var targetHits = Physics.RaycastAll(obj.transform.position, targetDir, targetDist.magnitude, LayerMask.GetMask(new string[] { "Wall" }));
		if (targetHits.Length > 0)
			return false;
		else
			return true;
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

	public static Orientation RandomOrientation() {
		return cardinalToOrient.Values.ToList()[Random.Range(0, 3)];
	}

	public static int GetNumericKeyDown() {
		if (Input.GetKeyDown(KeyCode.Alpha0)) return 10;
		if (Input.GetKeyDown(KeyCode.Alpha1)) return 1;
		if (Input.GetKeyDown(KeyCode.Alpha2)) return 2;
		if (Input.GetKeyDown(KeyCode.Alpha3)) return 3;
		if (Input.GetKeyDown(KeyCode.Alpha4)) return 4;
		if (Input.GetKeyDown(KeyCode.Alpha5)) return 5;
		if (Input.GetKeyDown(KeyCode.Alpha6)) return 6;
		if (Input.GetKeyDown(KeyCode.Alpha7)) return 7;
		if (Input.GetKeyDown(KeyCode.Alpha8)) return 8;
		if (Input.GetKeyDown(KeyCode.Alpha9)) return 9;
		return -1;
	}

	public static Bounds GetCameraBounds() {
		var camera = Camera.main;
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		Bounds bounds = new Bounds(
			camera.transform.position,
			new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
		return bounds;	
	}

	public static Color RandomColor() {
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
	}

	public static T GetRandom<T>(List<T> list) {
		return list[Random.Range(0, list.Count-1)];
	}

	public static List<T> Shuffle<T>(List<T> srcList) {
		var list = new List<T>(srcList);
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range(0, n);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  

		return list;
	}
}