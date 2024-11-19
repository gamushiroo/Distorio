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
    public static Vector3 ToVector3 (double x, double y, double z) => new((float)x, (float)y, (float)z);
}