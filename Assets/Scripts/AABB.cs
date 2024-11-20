using System;
using UnityEngine;
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
        double x1 = minX;
        double y1 = minY;
        double z1 = minZ;
        double x2 = maxX;
        double y2 = maxY;
        double z2 = maxZ;
        if (x < 0.0D) {
            x1 += x;
        } else {
            x2 += x;
        }
        if (y < 0.0D) {
            y1 += y;
        } else {
            y2 += y;
        }
        if (z < 0.0D) {
            z1 += z;
        } else {
            z2 += z;
        }
        return new(x1, y1, z1, x2, y2, z2);
    }
    public readonly bool IntersectsWith (AABB other) {
        return other.minX < maxX && minX < other.maxX && other.minY < maxY && minY < other.maxY && other.minZ < maxZ && minZ < other.maxZ;
    }
    public readonly bool IntersectsWith (Vector3Int pos) {
        return IntersectsWith(new AABB(pos.x, pos.y, pos.z, pos.x + 1, pos.y + 1, pos.z + 1));
    }
    public readonly double CalculateXOffset (double offsetX, AABB other) {
        if (minY < other.maxY && maxY > other.minY && minZ < other.maxZ && maxZ > other.minZ) {
            if (offsetX > 0.0D && maxX <= other.minX) {
                double diff = other.minX - maxX;
                if (diff < offsetX) {
                    offsetX = diff;
                }
            } else if (offsetX < 0.0D && minX >= other.maxX) {
                double diff = other.maxX - minX;
                if (diff > offsetX) {
                    offsetX = diff;
                }
            }
        }
        return offsetX;
    }
    public readonly double CalculateYOffset (double offsetY, AABB other) {
        if (minX < other.maxX && maxX > other.minX && minZ < other.maxZ && maxZ > other.minZ) {
            if (offsetY > 0.0D && maxY <= other.minY) {
                double diff = other.minY - maxY;
                if (diff < offsetY) {
                    offsetY = diff;
                }
            } else if (offsetY < 0.0D && minY >= other.maxY) {
                double diff = other.maxY - minY;
                if (diff > offsetY) {
                    offsetY = diff;
                }
            }
        }
        return offsetY;
    }
    public readonly double CalculateZOffset (double offsetZ, AABB other) {
        if (minX < other.maxX && maxX > other.minX && minY < other.maxY && maxY > other.minY) {
            if (offsetZ > 0.0D && maxZ <= other.minZ) {
                double diff = other.minZ - maxZ;
                if (diff < offsetZ) {
                    offsetZ = diff;
                }
            } else if (offsetZ < 0.0D && minZ >= other.maxZ) {
                double diff = other.maxZ - minZ;
                if (diff > offsetZ) {
                    offsetZ = diff;
                }
            }
        }
        return offsetZ;
    }
}
