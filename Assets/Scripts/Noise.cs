using UnityEngine;

public static class Noise {
    public static Vector2 offset;
    public static float Get2DPerlin (Vector2 pos, float scale) {
        return Mathf.PerlinNoise(pos.x * scale + offset.x, pos.y * scale + offset.y) - 0.5f;
    }
    public static float Get3DPerlin (Vector3 pos, float scale) {
        pos *= scale;
        float AB = Mathf.PerlinNoise(pos.x, pos.y);
        float BC = Mathf.PerlinNoise(pos.y, pos.z);
        float AC = Mathf.PerlinNoise(pos.x, pos.z);
        float BA = Mathf.PerlinNoise(pos.y, pos.x);
        float CB = Mathf.PerlinNoise(pos.z, pos.y);
        float CA = Mathf.PerlinNoise(pos.z, pos.x);
        return (AB + BC + AC + BA + CB + CA) / 6;
    }
}
