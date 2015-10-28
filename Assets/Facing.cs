using UnityEngine;
using System;
using System.Collections;

public class InvalidFacingException : Exception {

}

[Serializable]
public struct Facing {
    [SerializeField]
    private int index;
    public static Facing up = new Facing(2);
    public static Facing down = new Facing(-2);
    public static Facing right = new Facing(1);
    public static Facing left = new Facing(-1);

    public static explicit operator Facing(int index) {
        return new Facing(index);
    }

    public static explicit operator int(Facing facing) {
        return facing.index;
    }

    public static explicit operator Facing(IntVector2 pos) {
        if (Math.Abs(pos.x) > Math.Abs(pos.y)) {
            if (pos.x > 0) return Facing.right;
            if (pos.x < 0) return Facing.left;
        } else if (Math.Abs(pos.x) < Math.Abs(pos.y)){
            if (pos.y > 0) return Facing.up;
            if (pos.y < 0) return Facing.down;
        }


        return Facing.up;
    }

    public static explicit operator Vector2(Facing facing) {
        return (Vector2)((IntVector2)facing);
    }

    public static Facing operator -(Facing facing) {
        if (facing == Facing.up)
            return Facing.down;
        if (facing == Facing.down)
            return Facing.up;
        if (facing == Facing.right)
            return Facing.left;
        if (facing == Facing.left)
            return Facing.right;
        throw new InvalidFacingException();
    }

    public static bool operator ==(Facing r1, Facing r2) {
        return r1.index == r2.index;
    }
    
    public static bool operator !=(Facing r1, Facing r2) {
        return r1.index != r2.index;
    }

    public override bool Equals(System.Object obj) {
        if (obj is Facing) return (Facing)obj == this;
        return false;
    }

    public override string ToString() {
        if (this == Facing.up)
            return "up";
        else if (this == Facing.down)
            return "down";
        else if (this == Facing.right)
            return "right";
        else if (this == Facing.left)
            return "left";

        return String.Format("Invalid facing: {0}", index);
    }

    public static Facing FromString(string s) {
        if (s == "up")
            return Facing.up;
        else if (s == "down")
            return Facing.down;
        else if (s == "right")
            return Facing.right;
        else if (s == "left")
            return Facing.left;

        return Facing.up;
    }

    public override int GetHashCode() {
        return index;
    }

    public Facing(int index) {
        if (index != -2 && index != -1 && index != 1 && index != 2)
            throw new ArgumentException(String.Format("{0} is not a valid facing index", index));
        this.index = index;
    }
}
