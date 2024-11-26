using System;
public struct AABB {
    public double minX;
    public double minY;
    public double minZ;
    public double maxX;
    public double maxY;
    public double maxZ;
    public AABB (double x1, double y1, double z1, double x2, double y2, double z2) {
        minX = Math.Min(x1, x2);
        minY = Math.Min(y1, y2);
        minZ = Math.Min(z1, z2);
        maxX = Math.Max(x1, x2);
        maxY = Math.Max(y1, y2);
        maxZ = Math.Max(z1, z2);
    }
    public readonly AABB AddCoord (double x, double y, double z) {
        return new(minX + Math.Min(0, x), minY + Math.Min(0, y), minZ + Math.Min(0, z), maxX + Math.Max(0, x), maxY + Math.Max(0, y), maxZ + Math.Max(0, z));
    }
    public readonly bool IntersectsWith (AABB other) {
        return minX < other.maxX && maxX > other.minX && minY < other.maxY && maxY > other.minY && minZ < other.maxZ && maxZ > other.minZ;
    }
    public readonly bool IntersectsWith (Vec3i pos) {
        return IntersectsWith(new AABB(pos.x, pos.y, pos.z, pos.x + 1, pos.y + 1, pos.z + 1));
    }
    public readonly double CalculateXOffset (double offset, AABB other) {
        if (minY < other.maxY && maxY > other.minY && minZ < other.maxZ && maxZ > other.minZ) {
            if (offset > 0.0D && maxX <= other.minX) {
                offset = Math.Min(other.minX - maxX, offset);
            } else if (offset < 0.0D && minX >= other.maxX) {
                offset = Math.Max(other.maxX - minX, offset);
            }
        }
        return offset;
    }
    public readonly double CalculateYOffset (double offset, AABB other) {
        if (minX < other.maxX && maxX > other.minX && minZ < other.maxZ && maxZ > other.minZ) {
            if (offset > 0.0D && maxY <= other.minY) {
                offset = Math.Min(other.minY - maxY, offset);
            } else if (offset < 0.0D && minY >= other.maxY) {
                offset = Math.Max(other.maxY - minY, offset);
            }
        }
        return offset;
    }
    public readonly double CalculateZOffset (double offset, AABB other) {
        if (minX < other.maxX && maxX > other.minX && minY < other.maxY && maxY > other.minY) {
            if (offset > 0.0D && maxZ <= other.minZ) {
                offset = Math.Min(other.minZ - maxZ, offset);
            } else if (offset < 0.0D && minZ >= other.maxZ) {
                offset = Math.Max(other.maxZ - minZ, offset);
            }
        }
        return offset;
    }
}
