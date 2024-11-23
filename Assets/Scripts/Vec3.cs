using UnityEngine;
public struct Vec3 {
    public double xCoord;
    public double yCoord;
    public double zCoord;
    public Vec3 (double x, double y, double z) {
        xCoord = x;
        yCoord = y;
        zCoord = z;
    }
    public static Vec3 operator * (Vec3 a, double b) => new(a.xCoord * b, a.yCoord * b, a.zCoord * b);
    public static Vector3 ToVector3 (double x, double y, double z) => new((float)x, (float)y, (float)z);
    public static Vec3 ToVec3 (Vector3 coord) => new(coord.x, coord.y, coord.z);
}