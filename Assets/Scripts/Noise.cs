using UnityEngine;

public static class Noise {


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
