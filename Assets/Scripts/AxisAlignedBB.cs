using System;

public class AxisAlignedBB {

    public double minX;
    public double minY;
    public double minZ;
    public double maxX;
    public double maxY;
    public double maxZ;

    public AxisAlignedBB (double x1, double y1, double z1, double x2, double y2, double z2) {
        minX = Math.Min(x1, x2);
        minY = Math.Min(y1, y2);
        minZ = Math.Min(z1, z2);
        maxX = Math.Max(x1, x2);
        maxY = Math.Max(y1, y2);
        maxZ = Math.Max(z1, z2);
    }
}
