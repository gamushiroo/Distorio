using System.Numerics;
using UnityEngine;

public struct Vec3i {
    public int x;
    public int y;
    public int z;
    public Vec3i (int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static Vec3i ToChunkCoord (double x, double y, double z) {
        return new(MathHelper.FloorDouble(x) >> 4, MathHelper.FloorDouble(y) >> 7, MathHelper.FloorDouble(z) >> 4);
    }
    public static Vec3i ToVec3i (Vector3Int v) => new(v.x, v.y, v.z);
    public readonly bool Equals (Vec3i other) {
        return x == other.x && y == other.y && z == other.z;
    }
}