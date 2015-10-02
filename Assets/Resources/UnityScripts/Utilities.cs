using UnityEngine;
using System;
using System.IO;
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

public struct IntVector2 {
    public int x;
    public int y;

    public static double Distance(IntVector2 v1, IntVector2 v2) {
        return Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.y - v2.y, 2));
    }

    public static IntVector2[] Neighbors(IntVector2 bp) {
        return new IntVector2[] {
            new IntVector2(bp.x-1, bp.y),
            new IntVector2(bp.x+1, bp.y),
            new IntVector2(bp.x, bp.y-1),
            new IntVector2(bp.x, bp.y+1)
        };
    }
    
    public static IntVector2[] NeighborsWithDiagonal(IntVector2 bp) {
        return new IntVector2[] {
            new IntVector2(bp.x-1, bp.y),
            new IntVector2(bp.x+1, bp.y),
            new IntVector2(bp.x, bp.y-1),
            new IntVector2(bp.x, bp.y+1),
            
            new IntVector2(bp.x-1, bp.y-1),
            new IntVector2(bp.x-1, bp.y+1),
            new IntVector2(bp.x+1, bp.y-1),
            new IntVector2(bp.x+1, bp.y+1)
        };
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

    public IntVector2(int x, int y, int z) {
        this.x = x;
        this.y = y;
    }
}

public class DebugUtil {
	public static void DrawRect(Transform transform, Rect rect) {
		var p1 = transform.TransformPoint(new Vector2(rect.xMin, rect.yMin));
		var p2 = transform.TransformPoint(new Vector2(rect.xMax, rect.yMin));
		var p3 = transform.TransformPoint(new Vector2(rect.xMax, rect.yMax));
		var p4 = transform.TransformPoint(new Vector2(rect.xMin, rect.yMax));

		Debug.DrawLine(p1, p2);
		Debug.DrawLine(p2, p3);
		Debug.DrawLine(p3, p4);
		Debug.DrawLine(p4, p1);
	}

	public static void DrawPath(List<Vector2> points) {
		for (var i = 1; i < points.Count; i++) {
			Debug.DrawLine(points[i-1], points[i], Color.green);
		}
	}
}

public class Util {
	public static IEnumerable<Vector2> RectCorners(Rect rect) {
		yield return new Vector2(rect.xMin, rect.yMin);
		yield return new Vector2(rect.xMax, rect.yMin);
		yield return new Vector2(rect.xMax, rect.yMax);
		yield return new Vector2(rect.xMin, rect.yMax);
	}

    public static RaycastHit[] ParticleCast(ParticleSystem ps) {
        var radius = 0.05f;
        var length = ps.startSpeed * ps.startLifetime;
        return Physics.SphereCastAll(ps.transform.position, radius, ps.transform.up, length);
    }

	public static IEnumerable<RaycastHit> ShipCast(Blockform form, Vector3 dest) {
		var head = form.transform.position + form.transform.TransformVector(new Vector2(0, form.blocks.maxX*Tile.worldSize));
		var targetVec = (dest - head);
		var hits = Physics.SphereCastAll(head, form.width, targetVec.normalized, targetVec.magnitude, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
		foreach (var hit in hits) {
			if (hit.rigidbody != form.rigidBody) {
				yield return hit;
			}
		}
	}


    public static bool TurretBlocked(Blockform form, Vector3 turretPos, Vector3 targetPos) {        
        foreach (var block in form.BlocksAtWorldPos(targetPos))
            return true;

        var targetDist = (targetPos - turretPos);
        var targetDir = targetDist.normalized;
        
        var targetHits = Physics.RaycastAll(turretPos, targetDir, targetDist.magnitude);
        foreach (var hit in targetHits) {
            if (hit.rigidbody == form.rigidBody) {
                return true;
            }
        }

        return false;
    }

    public static bool TurretBlocked(Blockform form, Vector3 turretPos, Vector3 targetPos, float radius) {    
        foreach (var block in form.BlocksAtWorldPos(targetPos))
            return true;

        var targetDist = (targetPos - turretPos);
        var targetDir = targetDist.normalized;
        
        var targetHits = Physics.SphereCastAll(turretPos, radius, targetDir, targetDist.magnitude, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
        foreach (var hit in targetHits) {
            if (hit.rigidbody == form.rigidBody && form.WorldToBlockPos(hit.collider.transform.position) != form.WorldToBlockPos(turretPos)) {
                return true;
            }
        }
        
        return false;
    }

	public static bool LineOfSight(Blockform form, Blockform target) {
		var targetVec = (target.transform.position - form.transform.position);
		var hits = Physics.RaycastAll(form.transform.position, targetVec.normalized, targetVec.magnitude, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
		foreach (var hit in hits) {
			if (hit.rigidbody != form.rigidBody && hit.rigidbody != target.rigidBody) {
				return false;
			}
		}

		return true;
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

    public static Bounds GetCameraBounds(Camera camera) {
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
        return list[Random.Range(0, list.Count)];
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

    public static string GetIdFromPath(string path) {
        return Path.GetFileNameWithoutExtension(path);
    }
}