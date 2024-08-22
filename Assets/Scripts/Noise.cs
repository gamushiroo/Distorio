using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    public static float Get2DPerlin (Vector2 position, float scale) {

        //return Mathf.PerlinNoise((position.x))

        return Mathf.PerlinNoise(position.x * scale + 10000, position.y * scale + 10000);


        /*
        float heigh = 73;
        heigh += Mathf.PerlinNoise(pos.x * 0.03f, pos.y * 0.03f) * 12;
        heigh += Mathf.PerlinNoise(pos.x * 0.005f, pos.y * 0.005f) * 8;
        heigh += Mathf.Clamp(Mathf.PerlinNoise(pos.x * 0.01f, pos.y * 0.01f) * 256 - 192, -16, 16);
        */

    }

    public static bool Get3DPerlin (Vector3 position, float scale, float threshold) {

        float x = ( position.x ) * scale;
        float y = ( position.y ) * scale;
        float z = ( position.z ) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return ( AB + BC + AC + BA + CB + CA ) / 6 > threshold;

    }
}
