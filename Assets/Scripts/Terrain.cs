using Unity.VisualScripting;
using UnityEngine;
public static class Terrain {
    public static byte GetTerrain (int x, int y, int z) {
        byte VoxelValue = y < 40 ? (byte)4 : (byte)0;


        switch (y - Mathf.FloorToInt(GetTerrainHeight(x, z))) {
            case < -4:
                VoxelValue = 3;
                break;
            case < -1:
                VoxelValue = 2;
                break;
            case < 0:
                VoxelValue = 1;
                break;
            default:
                break;
        }
        if (VoxelValue == 0 && Noise.Get3DPerlin(new(x, y, z), 0.05f) >= 0.5f) {
            VoxelValue = 6;
        }
        return VoxelValue;
    }
    private static float GetTerrainHeight (int x, int y) {
        Vector2 pos = new(x, y);
        float terrainHeight = 0;
        for (int i = 0; i < 4; i++) {
            terrainHeight += Noise.Get2DPerlin(pos, MyResources.terrainScale / Mathf.Pow(2, i));
        }
        return terrainHeight * MyResources.terrainHeight * (Mathf.Pow(2, Noise.Get2DPerlin(pos, 0.029F) * 4) + Mathf.Pow(2, Noise.Get2DPerlin(pos, 0.005F) * 4) / 2) / (Noise.Get2DPerlin(pos, 0.05f) / 5000 + 1) + MyResources.solidGroundHeight;
    }
}
