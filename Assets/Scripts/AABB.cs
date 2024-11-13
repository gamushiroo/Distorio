using System;
public class AABB {
    public double minX, minY, minZ, maxX, maxY, maxZ;
    public AABB (double x1, double y1, double z1, double x2, double y2, double z2) {
        minX = Math.Min(x1, x2);
        minY = Math.Min(y1, y2);
        minZ = Math.Min(z1, z2);
        maxX = Math.Max(x1, x2);
        maxY = Math.Max(y1, y2);
        maxZ = Math.Max(z1, z2);
    }
    public AABB Extend (double x, double y, double z) {
        double d0 = minX;
        double d1 = minY;
        double d2 = minZ;
        double d3 = maxX;
        double d4 = maxY;
        double d5 = maxZ;
        if (x > 0.0D) {
            d3 += x;
        } else {
            d0 += x;
        }
        if (y > 0.0D) {
            d4 += y;
        } else {
            d1 += y;
        }
        if (z > 0.0D) {
            d5 += z;
        } else {
            d2 += z;
        }
        return new(d0, d1, d2, d3, d4, d5);
    }
    public double CalculateXOffset (AABB other, double offsetX) {
        if (other.maxY > minY && other.minY < maxY && other.maxZ > minZ && other.minZ < maxZ) {
            if (offsetX > 0.0D && other.maxX <= minX) {
                double d1 = minX - other.maxX;
                if (d1 < offsetX) {
                    offsetX = d1;
                }
            } else if (offsetX < 0.0D && other.minX >= maxX) {
                double d0 = maxX - other.minX;
                if (d0 > offsetX) {
                    offsetX = d0;
                }
            }
        }
        return offsetX;
    }
    public double CalculateYOffset (AABB other, double offsetY) {
        if (other.maxX > minX && other.minX < maxX && other.maxZ > minZ && other.minZ < maxZ) {
            if (offsetY > 0.0D && other.maxY <= minY) {
                double d1 = minY - other.maxY;
                if (d1 < offsetY) {
                    offsetY = d1;
                }
            } else if (offsetY < 0.0D && other.minY >= maxY) {
                double d0 = maxY - other.minY;
                if (d0 > offsetY) {
                    offsetY = d0;
                }
            }
        }
        return offsetY;
    }
    public double CalculateZOffset (AABB other, double offsetZ) {
        if (other.maxX > minX && other.minX < maxX && other.maxY > minY && other.minY < maxY) {
            if (offsetZ > 0.0D && other.maxZ <= minZ) {
                double d1 = minZ - other.maxZ;
                if (d1 < offsetZ) {
                    offsetZ = d1;
                }
            } else if (offsetZ < 0.0D && other.minZ >= maxZ) {
                double d0 = maxZ - other.minZ;
                if (d0 > offsetZ) {
                    offsetZ = d0;
                }
            }
        }
        return offsetZ;
    }
}
