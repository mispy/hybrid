using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// IntVector2 is mainly used to represent positions in a 2D grid, such
/// as the BlockMap. Methods which correspond to the Unity Vector2 should
/// try to stay consistent with that API.
/// </summary>
public struct IntVector2 {    
    public static IntVector2 zero = new IntVector2(0, 0);
    public static IntVector2 up = new IntVector2(0, 1);
    public static IntVector2 down = new IntVector2(0, -1);
    public static IntVector2 right = new IntVector2(1, 0);
    public static IntVector2 left = new IntVector2(-1, 0);

    [SerializeField]
    public readonly int x;
    [SerializeField]
    public readonly int y;

    public IntVector2(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public IntVector2 normalized {
        get {
            return new IntVector2(Math.Sign(x), Math.Sign(y));
        }
    }

    /// <summary>
    /// Returns the distance between a and b.
    /// </summary>
    public static double Distance(IntVector2 a, IntVector2 b) {
        return Math.Sqrt(Math.Pow(a.x - a.x, 2) + Math.Pow(a.y - a.y, 2));
    }

    /// <summary>
    /// For a given position, get the neighboring positions in each
    /// of the four cardinal directions.
    /// </summary>
    /// <returns>Four neighboring positions.</returns>
    /// <param name="pos">Center position.</param>
    public static IEnumerable<IntVector2> Neighbors(IntVector2 pos) {
        yield return new IntVector2(pos.x-1, pos.y);
        yield return new IntVector2(pos.x+1, pos.y);
        yield return new IntVector2(pos.x, pos.y-1);
        yield return new IntVector2(pos.x, pos.y+1);
    }

    /// <summary>
    /// For a given position, get the neighboring positions in each
    /// of the four cardinal directions and also the four diagonals.
    /// </summary>
    /// <returns>Eight neighboring positions.</returns>
    /// <param name="pos">Center position.</param>
    public static IEnumerable<IntVector2> NeighborsWithDiagonal(IntVector2 pos) {
        yield return new IntVector2(pos.x-1, pos.y);
        yield return new IntVector2(pos.x+1, pos.y);
        yield return new IntVector2(pos.x, pos.y-1);
        yield return new IntVector2(pos.x, pos.y+1);

        yield return new IntVector2(pos.x-1, pos.y-1);
        yield return new IntVector2(pos.x-1, pos.y+1);
        yield return new IntVector2(pos.x+1, pos.y-1);
        yield return new IntVector2(pos.x+1, pos.y+1);
    }

    /// <summary>
    /// For a rectangle of given width and height, get all the 
    /// positions contained within it.
    /// </summary>
    /// <param name="pos">Bottom-left corner of rectangle.</param>
    /// <param name="width">Width of rectangle.</param>
    /// <param name="height">Height of rectangle.</param>
    public static IEnumerable<IntVector2> Rectangle(IntVector2 pos, int width, int height) {
        for (var i = pos.x; i < pos.x+width; i++) {
            for (var j = pos.y; j < pos.y+height; j++) {
                yield return new IntVector2(i, j);
            }
        }
    }

    /// <summary>
    /// Get the positions directly adjacent to a rectangle, not including the diagonals.
    /// 
    /// e.g. for a 3x3, returning the 'x's:
    /// 
    ///  xxx
    /// x***x
    /// x***x
    /// xp**x
    ///  xxx
    /// </summary>
    /// <returns>Iteration of the neighboring positions.</returns>
    /// <param name="pos">Bottom-left corner of rectangle.</param>
    /// <param name="width">Width of rectangle.</param>
    /// <param name="height">Height of rectangle.</param>
    public static IEnumerable<IntVector2> NeighborsOfRectangle(IntVector2 pos, int width, int height) {
        foreach (var facing in Facing.all) {
            foreach (var sidePos in SideOfRectangle(pos, width, height, facing)) {
                yield return sidePos+(IntVector2)facing;
            }
        }
    }

    /// <summary>
    /// Get the positions lining a particular side of a rectangle.
    /// 
    /// e.g. for a 3x3 returning the right side:
    /// 
    ///  **x
    ///  **x
    ///  p*x
    /// 
    /// </summary>
    /// <returns>Iteration of the side positions.</returns>
    /// <param name="pos">Bottom-left corner of rectangle</param>
    /// <param name="width">Width of rectangle.</param>
    /// <param name="height">Height of rectangle.</param>
    /// <param name="side">Side of the rectangle you want.</param>
    public static IEnumerable<IntVector2> SideOfRectangle(IntVector2 pos, int width, int height, Facing side) {
        if (side == Facing.up || side == Facing.down) {
            var y = (side == Facing.up ? pos.y+height-1 : pos.y);
            for (var i = pos.x; i < pos.x+width; i++) {
                yield return new IntVector2(i, y);
            }
        } else if (side == Facing.left || side == Facing.right) {
            var x = (side == Facing.right ? pos.x+width-1 : pos.x);
            for (var j = pos.y; j < pos.y+height; j++) {
                yield return new IntVector2(x, j);
            }
        }
    }

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

    public static bool operator ==(IntVector2 v1, IntVector2 v2) {
        return v1.x == v2.x && v1.y == v2.y;
    }

    public override bool Equals(object obj) {
        return this == (IntVector2)obj;
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

    public override string ToString() {
        return String.Format("IntVector2<{0}, {1}>", x, y);
    }
}