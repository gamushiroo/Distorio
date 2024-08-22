using System.Collections.Generic;
using UnityEngine;

public static class Data {

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize {

        get { return 1f / TextureAtlasSizeInBlocks; }

    }

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int ChunkLoadRange = 4;
    public static readonly int TextureSize = 16;
    //public static readonly float playerSpeed = 20f;
    public static readonly float playerSpeed = 4.5f;
    public static readonly float mouseSens = 100;

    public static readonly EntityData player = new EntityData("player", new(0.75f, 1.75f, 0.75f), 100, 2000);

    public static readonly int[,] terrain = new int[3, 2] {

        {0, 1},
        {1, 2},
        {4, 3}

    };

    public static readonly Vector3[] voxelVerts = new Vector3[8] {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)

    };

    public static readonly byte[,,] trees = new byte[5, 6, 5] {

        {{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 5, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 } },
        {{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 5, 5, 5, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 } },
        {{0, 0, 4, 0, 0 },{0, 0, 4, 0, 0 },{5, 5, 4, 5, 5 },{0, 0, 4, 0, 0 },{0, 0, 5, 0, 0 },{0, 0, 5, 0, 0 } },
        {{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 5, 5, 5, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 } },
        {{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 5, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 },{0, 0, 0, 0, 0 } }

    };


    public static readonly Vector3Int[] faceChecks = new Vector3Int[6] {

        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0)

    };

    public static readonly int[,] voxelTris = new int[6, 4] {

        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6}  // Right Face

    };

    public static readonly int[] order = new int[6]{

        0, 1, 2, 2, 1, 3

    };

    public static readonly Vector2[] voxelUVs = new Vector2[4] {

        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)

    };

    public static bool IsInChunk (Vector3 pos) {

        return !( pos.x < 0 || pos.x >= ChunkWidth || pos.y < 0 || pos.y >= ChunkHeight || pos.z < 0 || pos.z >= ChunkWidth );

    }

    public static Vector2 TexturePos (int ID) {

        return new(ID % TextureSize, TextureSize - 1 - ID / TextureSize);

    }

    public static ChunkVoxel Vector3ToChunkVoxel (Vector3 pos) {

        Vector2Int ddd = new(Mathf.FloorToInt(pos.x / ChunkWidth), Mathf.FloorToInt(pos.z / ChunkWidth));


        int x = pos.x >= 0 ? Mathf.FloorToInt(pos.x) % ChunkWidth : Mathf.CeilToInt(pos.x) % ChunkWidth + ChunkWidth - 1;
        int y = pos.y >= 0 ? Mathf.FloorToInt(pos.y) % ChunkHeight : Mathf.CeilToInt(pos.y) % ChunkHeight + ChunkHeight - 1;
        int z = pos.z >= 0 ? Mathf.FloorToInt(pos.z) % ChunkWidth : Mathf.CeilToInt(pos.z) % ChunkWidth + ChunkWidth - 1;

        return new(ddd, new(x, y, z));

    }

    public static int GetScrool () {

        if (Input.mouseScrollDelta.y > 0) {
            return 1;
        }
        else if (Input.mouseScrollDelta.y < 0) {
            return -1;
        }
        else {
            return 0;
        }
    }

    public static Vector3Int PublicLocationDerect (ChunkVoxel pos) => pos.v + new Vector3Int(pos.c.x * ChunkWidth,0, pos.c.y * ChunkWidth);


    public static Vector3 GetPlayerVel () {

        int x = 0;
        int y = 0;
        int z = 0;
        int dash = 0;

        if (Input.GetKey(KeyCode.D))
            x++;
        if (Input.GetKey(KeyCode.A))
            x--;
        if (Input.GetKey(KeyCode.W))
            z++;
        if (Input.GetKey(KeyCode.S))
            z--;
        if (z == 1 && Input.GetKey(KeyCode.LeftShift))
            dash++;

        return new Vector3(x, y, z).normalized + dash * Vector3.forward;

    }
}