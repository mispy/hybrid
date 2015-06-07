using UnityEngine;
using System;

public class PoolBehaviour : MonoBehaviour, ISerializationCallbackReceiver {
	public virtual void OnCreate() { }
	public virtual void OnRecycle() { }
	public virtual void OnPreSerialize() { }
	public virtual void OnAnyDeserialize() { }
	public virtual void OnSecondDeserialize() { }
	public virtual void OnAnyEnable() { }
	public virtual void OnFirstEnable() { }
	public virtual void OnAwake() { }
	public virtual void OnRestore() { }

	public bool isAwake = false;
	public bool hasBeenEnabled = false;

	public void Awake() {
		OnAwake();
		isAwake = true;
	}

	public void OnBeforeSerialize() {
	}

	public void OnAfterDeserialize() {
		OnAnyDeserialize();

		if (isAwake) {
			OnSecondDeserialize();
		}
	}

	public void OnEnable() {		
		OnAnyEnable();

		if (!hasBeenEnabled) {
			OnFirstEnable();
		} else {
			OnRestore();
		}
		hasBeenEnabled = true;
	}

	private Block _block;
	public Block block {
		get { return _block; }
		set { _block = value; }
	}
}

public interface IBlockComponent {
	GameObject gameObject { get; }
	Transform transform { get; }
	Block block { get; set; }
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
}