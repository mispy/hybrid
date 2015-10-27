using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class PoolBehaviour : MonoBehaviour {
    public virtual void OnCreate() { }
    public virtual void OnRecycle() { }

    public void DestroyChildren() {
        foreach (Transform child in transform) {
            Pool.Recycle(child.gameObject);
        }
    }
}

public struct IntRect {
    public int minX;
    public int minY;
    public int maxX;
    public int maxY;

    public IntRect(IntVector2 pos1, IntVector2 pos2) {
        minX = Math.Min(pos1.x, pos2.x);
        minY = Math.Min(pos1.y, pos2.y);
        maxX = Math.Max(pos1.x, pos2.x);
        maxY = Math.Max(pos1.y, pos2.y);
    }

    public bool Contains(IntVector2 pos) {
        return (pos.x >= minX && pos.y >= minY && pos.x <= maxX && pos.y <= maxY);
    }
}

[Serializable]
public struct IntVector2 : ISaveAsString {    
    [SerializeField]
    public int x;
    [SerializeField]
    public int y;

    public static explicit operator IntVector2(Facing facing) {
        if (facing == Facing.up)
            return IntVector2.up;
        else if (facing == Facing.down)
            return IntVector2.down;
        else if (facing == Facing.left)
            return IntVector2.left;
        else if (facing == Facing.right)
            return IntVector2.right;
        else
            throw new ArgumentException(facing.ToString());
    }
   
    public static explicit operator Vector2(IntVector2 pos) {
        return new Vector2(pos.x, pos.y);   
    }

    public static IntVector2 zero = new IntVector2(0, 0);
    public static IntVector2 up = new IntVector2(0, 1);
    public static IntVector2 down = new IntVector2(0, -1);
    public static IntVector2 right = new IntVector2(1, 0);
    public static IntVector2 left = new IntVector2(-1, 0);

    public static double Distance(IntVector2 v1, IntVector2 v2) {
        return Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.y - v2.y, 2));
    }

    public static IEnumerable<IntVector2> Neighbors(IntVector2 bp) {
        yield return new IntVector2(bp.x-1, bp.y);
        yield return new IntVector2(bp.x+1, bp.y);
        yield return new IntVector2(bp.x, bp.y-1);
        yield return new IntVector2(bp.x, bp.y+1);
    }
    
    public static IEnumerable<IntVector2> NeighborsWithDiagonal(IntVector2 bp) {
        yield return new IntVector2(bp.x-1, bp.y);
        yield return new IntVector2(bp.x+1, bp.y);
        yield return new IntVector2(bp.x, bp.y-1);
        yield return new IntVector2(bp.x, bp.y+1);
        
        yield return new IntVector2(bp.x-1, bp.y-1);
        yield return new IntVector2(bp.x-1, bp.y+1);
        yield return new IntVector2(bp.x+1, bp.y-1);
        yield return new IntVector2(bp.x+1, bp.y+1);
    }

    public static bool operator ==(IntVector2 v1, IntVector2 v2) {
		return v1.x == v2.x && v1.y == v2.y;
    }

    public static bool operator !=(IntVector2 v1, IntVector2 v2) {
		return v1.x != v2.x || v1.y != v2.y;
    }

    public static IntVector2 operator +(IntVector2 v1, IntVector2 v2) {
        return new IntVector2(v1.x+v2.x, v1.y+v2.y);
    }

    public static IntVector2 operator -(IntVector2 v1, IntVector2 v2) {
        return new IntVector2(v1.x-v2.x, v1.y-v2.y);
    }

    public static IntVector2 operator -(IntVector2 v) {
        return new IntVector2(-v.x, -v.y);
    }

    public IntVector2 normalized {
        get {
            return new IntVector2(Math.Sign(x), Math.Sign(y));
        }
    }

    public override string ToString() {
        return String.Format("{0} {1}", x, y);
    }

    public static IntVector2 FromString(string s) {
        var ints = s.Split();
        return new IntVector2(Int32.Parse(ints[0]), Int32.Parse(ints[1]));
    }

    public override int GetHashCode()
    {
		return (x << 32) + y;
    }

	public override bool Equals(object obj) {
		return this == (IntVector2)obj;
	}
    
    public IntVector2(int x, int y) {
        this.x = x;
        this.y = y;
    }
}

public class GalaxyPos {
    public Star star { get; private set; }
    public Vector2 vec { get; private set; }

    public float x {
        get {
            return vec.x;
        }
    }

    public float y {
        get {
            return vec.y;
        }
    }

    public static implicit operator Vector2(GalaxyPos pos) {
        return pos.vec;
    }

    public GalaxyPos(float x, float y) {
        this.star = null;
        this.vec = new Vector2(x, y);
    }

    public GalaxyPos(Star star, float x, float y) {
        this.star = star;
        this.vec = new Vector2(x, y);
    }

    public GalaxyPos(Star star, Vector2 vec) {
        this.star = star;
        this.vec = vec;
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

	public static void DrawBounds(Transform transform, Bounds bounds) {
		var p1 = transform.TransformPoint(new Vector2(bounds.min.x, bounds.min.y));
		var p2 = transform.TransformPoint(new Vector2(bounds.max.x, bounds.min.y));
		var p3 = transform.TransformPoint(new Vector2(bounds.max.x, bounds.max.y));
		var p4 = transform.TransformPoint(new Vector2(bounds.min.x, bounds.max.y));
		
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

	public static IEnumerable<RaycastHit> ShipCast(Blockform form, Vector3 endPos) {
		var head = form.transform.position + form.transform.TransformVector(new Vector2(0, form.blocks.maxX*Tile.worldSize));
		return ShipCast(form, head, endPos);
	}

	public static IEnumerable<RaycastHit> ShipCast(Blockform form, Vector3 startPos, Vector3 endPos) {
		var targetVec = (endPos - startPos);
		var hits = Physics.SphereCastAll(startPos, form.width, targetVec.normalized, targetVec.magnitude, LayerMask.GetMask(new string[] { "Bounds" }));
		foreach (var hit in hits) {
			if (hit.rigidbody != form.rigidBody) {
				yield return hit;
			}
		}
	}

	public static IEnumerable<Blockform> ShipsInRadius(Vector2 pos, float radius) {
		foreach (var hit in Physics.OverlapSphere(pos, radius, SpaceLayer.ShipBounds)) {
			var form = hit.attachedRigidbody.gameObject.GetComponent<Blockform>();
			if (form != null) yield return form;
		}
	}

/*    public static bool TurretBlocked(Blockform form, Vector3 turretPos, Vector3 targetPos) {        
        foreach (var block in form.BlocksAtWorldPos(targetPos))
            return true;

        var targetDist = (targetPos - turretPos);
        var targetDir = targetDist.normalized;
        
		var targetHits = Physics.RaycastAll(turretPos, targetDir, targetDist.magnitude, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
        foreach (var hit in targetHits) {
            if (hit.rigidbody == form.rigidBody) {
                return true;
            }
        }

        return false;
    }*/

    public static bool TurretBlocked(Blockform form, Vector3 turretPos, Vector3 targetPos, float radius = 0.5f) {    
        foreach (var block in form.BlocksAtWorldPos(targetPos))
            return true;

		var turretBlockPos = form.WorldToBlockPos(turretPos);
        var targetDir = (targetPos - turretPos).normalized;
        
        var targetHits = Physics.SphereCastAll(turretPos, radius, targetDir, form.length, LayerMask.GetMask(new string[] { "Wall", "Floor" }));
        foreach (var hit in targetHits) {
            if (hit.rigidbody == form.rigidBody && form.LocalToBlockPos(hit.collider.transform.localPosition) != turretBlockPos) {
                return true;
            }
        }
        
        return false;
    }


    private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

    // Bresenham's line algorithm
    public static IEnumerable<IntVector2> LineBetween(IntVector2 start, IntVector2 end) {
        var x0 = start.x;
        var y0 = start.y;
        var x1 = end.x;
        var y1 = end.y;

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
        if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
        int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;
        
        for (int x = x0; x <= x1; ++x)
        {
            if (steep)
                yield return new IntVector2(y, x);
            else
                yield return new IntVector2(x, y);
            err = err - dY;
            if (err < 0) { y += ystep;  err += dX; }
        }
    }

/*        real deltax := x1 - x0
            real deltay := y1 - y0
                real error := 0
                real deltaerr := abs (deltay / deltax)    // Assume deltax != 0 (line is not vertical),
                // note that this division needs to be done in a way that preserves the fractional part
                int y := y0
                for x from x0 to x1
                    plot(x,y)
                        error := error + deltaerr
                        while error ≥ 0.5 then
                            plot(x, y)
                                y := y + sign(y1 - y0)
                                error := error - 1.0*/


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

    public static Vector2 Cardinalize(Vector2 vec) {
        var normal = vec.normalized;
        return cardinals.OrderBy((c) => Vector2.Distance(c, normal)).First();
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

    public static T GetRandom<T>(T[] list) {
        return list[Random.Range(0, list.Length)];
    }

    public static T GetRandom<T>(HashSet<T> list) {
        return list.ToList()[Random.Range(0, list.Count)];
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

	public static float GetPathLength(List<Vector2> path) {
		float length = 0f;
		for (var i = 1; i < path.Count; i++)
			length += Vector2.Distance(path[i-1], path[i]);
		return length;
	}

	public static Vector2 PathLerp(List<Vector2> path, float amount) {
		var length = Util.GetPathLength(path);
		float distTraveled = 0f;
		for (var i = 1; i < path.Count; i++) {
			float dist = Vector2.Distance(path[i-1], path[i]);
			if (distTraveled+dist > length*amount) {
				return Vector2.Lerp(path[i-1], path[i], (amount*length - distTraveled)/dist);
			}

			distTraveled += dist;
		}

		return path.Last();
	}

    
    public static Block AdjoiningBlock(Block block, IntVector2 pos, BlockMap blocks) {
        if (block.type.canRotate && block.layer == BlockLayer.Base) {
            // Base-level rotating blocks must attach to the block behind them
            return blocks[pos - (IntVector2)block.facing, BlockLayer.Base];
        } else {
            foreach (var neighbor in IntVector2.Neighbors(pos)) {
                var adjoining = blocks[neighbor, BlockLayer.Base];
                if (adjoining != null) return adjoining;
            }
        }

        return null;
    }

    public static Block AdjoiningBlock(Block block, IntVector2 pos) {
        return AdjoiningBlock(block, pos, block.ship.blocks);
    }

    public static Block AdjoiningBlock(Block block) {
        return AdjoiningBlock(block, block.pos, block.ship.blocks);
    }

    public static Vector2 TipPosition(Block block) {
        return (Vector2)block.gameObject.transform.TransformPoint(Vector2.up*Tile.worldSize);
    }

    public static Vector2 RandomDirection() {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public static void DestroyChildrenImmediate(Transform transform) {
        List<Transform> toDestroy = new List<Transform>();

        foreach (Transform child in transform) {
            toDestroy.Add(child);
        }


        foreach (var child in toDestroy) {
            GameObject.DestroyImmediate(child.gameObject);
        }
    }
}