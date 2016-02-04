using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public interface IOnCreate {
    void OnCreate();
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
public struct IntVector3 {    
    [SerializeField]
    public int x;
    [SerializeField]
    public int y;
    [SerializeField]
    public int z;
    
    public static explicit operator IntVector3(Facing facing) {
        if (facing == Facing.up)
            return IntVector3.up;
        else if (facing == Facing.down)
            return IntVector3.down;
        else if (facing == Facing.left)
            return IntVector3.left;
        else if (facing == Facing.right)
            return IntVector3.right;
        else
            throw new ArgumentException(facing.ToString());
    }
    
    
    public static explicit operator Vector3(IntVector3 pos) {
        return new Vector3(pos.x, pos.y, pos.z);   
    }
    
    public static IntVector3 zero = new IntVector3(0, 0, 0);
    public static IntVector3 up = new IntVector3(0, 1, 0);
    public static IntVector3 down = new IntVector3(0, -1, 0);
    public static IntVector3 right = new IntVector3(1, 0, 0);
    public static IntVector3 left = new IntVector3(-1, 0, 0);
    
    public static double Distance(IntVector3 v1, IntVector3 v2) {
        return Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.y - v2.y, 2) + Math.Pow(v1.z - v2.z, 2));
    }

    public static bool operator ==(IntVector3 v1, IntVector3 v2) {
        return v1.x == v2.x && v1.y == v2.y & v1.z == v2.z;
    }
    
    public static bool operator !=(IntVector3 v1, IntVector3 v2) {
        return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
    }
    
    public static IntVector3 operator +(IntVector3 v1, IntVector3 v2) {
        return new IntVector3(v1.x+v2.x, v1.y+v2.y, v1.z+v2.z);
    }
    
    public static IntVector3 operator -(IntVector3 v1, IntVector3 v2) {
        return new IntVector3(v1.x-v2.x, v1.y-v2.y, v1.z-v2.z);
    }
    
    public static IntVector3 operator -(IntVector3 v) {
        return new IntVector3(-v.x, -v.y, -v.z);
    }
    
    public IntVector3 normalized {
        get {
            return new IntVector3(Math.Sign(x), Math.Sign(y), Math.Sign(z));
        }
    }
    
    public override string ToString() {
        return String.Format("{0} {1} {2}", x, y);
    }
    
    public static IntVector3 FromString(string s) {
        var ints = s.Split();
        return new IntVector3(Int32.Parse(ints[0]), Int32.Parse(ints[1]), Int32.Parse(ints[2]));
    }
    
    public override int GetHashCode()
    {
        return (x << 32) + y;
    }
    
    public override bool Equals(object obj) {
        return this == (IntVector3)obj;
    }
    
    public IntVector3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}



public class DebugUtil {
    public static void DrawPoint(Transform transform, Vector3 center) {
        Debug.DrawLine(center + -transform.right, center + transform.right, Color.green);
        Debug.DrawLine(center + transform.up, center + -transform.up, Color.green);
    }

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

public class ExtendedBinaryWriter : BinaryWriter {
    public ExtendedBinaryWriter(Stream output) : base(output) {

    }

    public virtual void Write(IntVector2 pos) {
        Write((short)pos.x);
        Write((short)pos.y);
    }

    public virtual void Write(IntVector3 pos) {
        Write((short)pos.x);
        Write((short)pos.y);
        Write((short)pos.z);
    }

    public virtual void Write(Vector2 pos) {
        Write(pos.x);
        Write(pos.y);
    }

    public virtual void Write(Vector3 pos) {
        Write(pos.x);
        Write(pos.y);
        Write(pos.z);
    }

    public virtual void Write(Quaternion quat) {
        Write(quat.x);
        Write(quat.y);
        Write(quat.z);
        Write(quat.w);
    }

    public virtual void Write(GUID guid) {
        Write(guid.value);
    }        
}

public interface ISaveable {
    void OnSave(SaveWriter save);
    void OnLoad(SaveReader save);
}

public class SaveWriter : ExtendedBinaryWriter {
    public SaveWriter(Stream output) : base(output) {

    }

    public virtual void Write(ISaveable obj) {
        obj.OnSave(this);
    }

    public virtual void Write(BlockType type) {
        Write(type.id);
    }

    public virtual void Write(Block block) {
        Write(block.type);
        Write(block.blockPos);
        if (block.type.canRotate)
            Write(block.facing.index);
    }
}

public class SaveReader : ExtendedBinaryReader {
    public SaveReader(Stream output) : base(output) {

    }

    public virtual void ReadSaveable(ISaveable obj) {
        obj.OnLoad(this);
    }

    public virtual BlockType ReadBlockType() {
        return BlockType.FromId(ReadString());
    }

    public virtual Block ReadBlock() {
        Block block;
        try {
            block = new Block(ReadBlockType());
        } catch (KeyNotFoundException ex) {
            ReadIntVector3();
            return null;
        }

        var pos = ReadIntVector3();
        block.blockPos = pos;
        if (block.type.canRotate)
            block.facing = (Facing)ReadInt32();
        return block;
    }        
}


public class MispyNetworkWriter : ExtendedBinaryWriter {
    public MispyNetworkWriter(Stream output) : base(output) {

    }

    public virtual void Write(PoolBehaviour net) {
        if (net == null) {
            Write(0);
        } else if (net.guid.value == null) {
            throw new ArgumentException(String.Format("Cannot write reference to component {0} of {1} without guid", net.GetType().Name, net.gameObject.name));
        } else {
            Write(net.guid);
        }
    }


    public virtual void Write(BlockType type) {
        Write(Game.GetPrefabIndex(type.gameObject));
    }

    public virtual void Write(Block block) {
        if (block == null) {
            Write(-1);
            return;
        }

        Write(Game.GetPrefabIndex(block.type.gameObject));
        Write(block.blockPos);
        if (block.type.canRotate)
            Write(block.facing.index);
    }
}

public class ExtendedBinaryReader : BinaryReader {
    public ExtendedBinaryReader(Stream input) : base(input) {
    }

    public virtual IntVector2 ReadIntVector2() {
        var x = ReadInt16();
        var y = ReadInt16();
        return new IntVector2(x, y);
    }

    public virtual IntVector3 ReadIntVector3() {
        var x = ReadInt16();
        var y = ReadInt16();
        var z = ReadInt16();
        return new IntVector3(x, y, z);
    }

    public virtual Vector2 ReadVector2() {
        var x = ReadSingle();
        var y = ReadSingle();
        return new Vector2(x, y);
    }

    public virtual Vector3 ReadVector3() {
        var x = ReadSingle();
        var y = ReadSingle();
        var z = ReadSingle();
        return new Vector3(x, y, z);
    }

    public virtual Quaternion ReadQuaternion() {
        var x = ReadSingle();
        var y = ReadSingle();
        var z = ReadSingle();
        var w = ReadSingle();
        return new Quaternion(x, y, z, w);
    }
}

public class MispyNetworkReader : ExtendedBinaryReader {
    public MispyNetworkReader(Stream input) : base(input) {
    }

    public virtual GUID ReadGUID() {
        var guid = new GUID(ReadInt32());
        if (!guid.isValid) {
            throw new ArgumentException(String.Format("Invalid GUID {0} read from binary stream", guid));
        }
        return guid;
    }

    public virtual T ReadComponent<T>() {
        var val = ReadInt32();
        if (val == 0)
            return default(T);
        else {
            var guid = new GUID(val);
            return SpaceNetwork.nets[guid].GetComponent<T>();
        }
    }

    public virtual BlockType ReadBlockType() {
        var prefabIndex = ReadInt32();
        return Game.PrefabFromIndex(prefabIndex).GetComponent<BlockType>();
    }

    public virtual Block ReadBlock() {
        var prefabIndex = ReadInt32();
        if (prefabIndex == -1) return null;

        var block = new Block(Game.PrefabFromIndex(prefabIndex).GetComponent<BlockType>());
        var pos = ReadIntVector3();
        block.blockPos = pos;
        if (block.type.canRotate)
            block.facing = (Facing)ReadInt32();
        return block;
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

    public static T NetworkFind<T>(uint id) {
        var netId = new NetworkInstanceId(id);
        var obj = ClientScene.FindLocalObject(netId);
        return obj.GetComponent<T>();
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

    public static bool TurretBlocked(Blockform form, Vector3 turretPos, Vector3 targetPos, float radius = 0.2f) {    
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