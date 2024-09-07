using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    public static Queue<VoxelAndPos> MakeCactus (Vector3Int position) {

        Queue<VoxelAndPos> queue = new();

        for (int _y = 0; _y < 4; _y++) {
            queue.Enqueue(new VoxelAndPos(Data.Vector3ToChunkVoxel(new Vector3Int(0, _y, 0) + position + Vector3.one * 0.5f), 11));
        }

        return queue;

    }

    public static Queue<VoxelAndPos> MakeFurnace (Vector3Int position) {

        Queue<VoxelAndPos> queue = new();

        for (int _x = 0; _x < 2; _x++) {
            for (int _y = 0; _y < 3; _y++) {
                for (int _z = 0; _z < 2; _z++) {
                    Vector3Int pos = new Vector3Int(_x, _y, _z) + position;
                    queue.Enqueue(new VoxelAndPos(Data.Vector3ToChunkVoxel(pos + Vector3.one * 0.5f ), 9));
                }
            }
        }

        return queue;

    }
    public static Queue<VoxelAndPos> MakeTree (Vector3Int position) {
        Queue<VoxelAndPos> queue = new();
        for (int _x = 0; _x < 5; _x++) {
            for (int _y = 0; _y < 7; _y++) {
                for (int _z = 0; _z < 5; _z++) {
                    if (Data.trees[_x, _y, _z] != 0) {
                        Vector3Int pos = new Vector3Int(_x, _y, _z) + position;
                        queue.Enqueue(new VoxelAndPos(Data.Vector3ToChunkVoxel(pos + Vector3.one * 0.5f + new Vector3Int(-2, 0, -2)), Data.trees[_x, _y, _z]));
                    }
                }
            }
        }
        return queue;
    }
}