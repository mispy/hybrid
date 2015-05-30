public struct IntVector2 {
	public int x;
	public int y;

	public static bool operator ==(IntVector2 v1, IntVector2 v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}

	public static bool operator !=(IntVector2 v1, IntVector2 v2) {
		return v1.x != v2.x || v1.x != v2.x;
	}
	
	public IntVector2(int x, int y) {
		this.x = x;
		this.y = y;
	}
}