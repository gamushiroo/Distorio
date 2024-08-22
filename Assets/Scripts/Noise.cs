using UnityEngine;

public static class Noise {

    public static float Get2DPerlin (Vector2 position, float scale) {

        return Mathf.PerlinNoise(position.x * scale + 10000, position.y * scale + 10000);

        //heigh += Mathf.Clamp(Mathf.PerlinNoise(pos.x * 0.01f, pos.y * 0.01f) * 256 - 192, -16, 16);

    }

    public static bool Get3DPerlin (Vector3 pos, float scale, float threshold) {

        pos *= scale;

        float AB = Mathf.PerlinNoise(pos.x, pos.y);
        float BC = Mathf.PerlinNoise(pos.y, pos.z);
        float AC = Mathf.PerlinNoise(pos.x, pos.z);
        float BA = Mathf.PerlinNoise(pos.y, pos.x);
        float CB = Mathf.PerlinNoise(pos.z, pos.y);
        float CA = Mathf.PerlinNoise(pos.z, pos.x);

        return ( AB + BC + AC + BA + CB + CA ) / 6 > threshold;

    }
}
