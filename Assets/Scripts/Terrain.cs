using System.Collections.Generic;
using UnityEngine;
using static Data;

public static class Terrain {
    /*
    public static Voxel[,,] TerrainData (Vector2Int chunkPos, World world) {

        Voxel[,,] voxelMap = new Voxel[ChunkWidth, ChunkHeight, ChunkWidth];

        for (int x = 0; x < ChunkWidth; x++) {
            for (int z = 0; z < ChunkWidth; z++) {

                Vector3 pos =  new Vector3(world.perlinNoiseOffset.x + x + chunkPos.x * ChunkWidth, 0, world.perlinNoiseOffset.y + z + chunkPos.y * ChunkWidth);

                float heigh = 73;
                heigh += Mathf.PerlinNoise(pos.x * 0.03f, pos.z * 0.03f) * 12;
                heigh += Mathf.PerlinNoise(pos.x * 0.005f, pos.z * 0.005f) * 8;
                heigh += Mathf.Clamp(Mathf.PerlinNoise(pos.x * 0.01f, pos.z * 0.01f) * 256 - 192, -16, 16);

                for (int y = 0; y < ChunkHeight; y++) {

                    voxelMap[x, y, z].id = (byte)(y < 64 ? 7 : 0);
                    voxelMap[x, y, z].group = 0;

                    for (int i = 0; i < 3; i++) {

                        if (y < heigh - terrain[i, 0])
                            voxelMap[x, y, z].id = (byte)terrain[i, 1];

                    }
                }
            }
        }

        for (int x = 0; x < ChunkWidth; x++) {
            for (int y = 0; y < ChunkHeight; y++) {
                for (int z = 0; z < ChunkWidth; z++) {
                    if (voxelMap[x, y, z].id == 1) {

                        for (int _x = -3; _x < 4; _x++) {
                            for (int _y = -3; _y < 4; _y++) {
                                for (int _z = -3; _z < 4; _z++) {

                                    Vector3Int pos = new(x + _x, y + _y, z + _z);

                                    if (IsInChunk(pos) && voxelMap[pos.x, pos.y, pos.z].id == 7) {

                                        voxelMap[x, y, z].id = 6;

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int x = 0; x < ChunkWidth; x++) {
            for (int y = 0; y < ChunkHeight; y++) {
                for (int z = 0; z < ChunkWidth; z++) {

                    if (Random.Range(0, 256) == 0) {


                        if (IsInChunk(new(x + 2, y - 1, z + 2)) && voxelMap[x + 2, y - 1, z + 2].id == 1 && IsInChunk(new(x + 2, y, z + 2)) && voxelMap[x + 2, y, z + 2].id == 0) {


                            int oo = world.GenerateKey();

                            world.noname.Add(oo, new List<ChunkVoxel>());

                            for (int _x = 0; _x < 5; _x++) {
                                for (int _y = 0; _y < 6; _y++) {
                                    for (int _z = 0; _z < 5; _z++) {

                                        Vector3Int pos = new(x + _x, y + _y, z + _z);

                                        if (IsInChunk(pos) && trees[_x, _y, _z] != 0) {

                                            voxelMap[pos.x, pos.y, pos.z].id = trees[_x, _y, _z];
                                            voxelMap[pos.x, pos.y, pos.z].group = oo;
                                            world.noname[oo].Add(new ChunkVoxel(chunkPos, pos));

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

       /*
        for (int x = 0; x < ChunkWidth; x++) {
            for (int y = 0; y < ChunkHeight; y++) {
                for (int z = 0; z < ChunkWidth; z++) {

                    for (int i = 0; i < world.aed.Count; i++) {

                        Vector3Int pos = new Vector3Int(chunkPos.x * ChunkWidth, 0, chunkPos.y * ChunkWidth) + new Vector3Int(x, y, z);

                        if (( world.aed[i] - pos ).magnitude < 8) {


                            voxelMap[x, y, z].id = 0;
                            voxelMap[x, y, z].group = 0;

                        }
                    }
                }
            }
        }
        return voxelMap;

    }
    
        */
}