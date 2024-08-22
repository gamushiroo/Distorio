using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    public static Queue<VoxelMod> MakeTree (Vector3Int position) {

        Queue<VoxelMod> queue = new();

        for (int _x = 0; _x < 5; _x++) {
            for (int _y = 0; _y < 6; _y++) {
                for (int _z = 0; _z < 5; _z++) {

                    Vector3Int pos = new Vector3Int(_x, _y, _z) + position;

                    if (Data.trees[_x, _y, _z] != 0) {

                        queue.Enqueue(new VoxelMod(Data.Vector3ToChunkVoxel(pos), Data.trees[_x, _y, _z]));

                    }
                }
            }
        }


        return queue;

    }
}

public class VoxelMod {

    public ChunkVoxel position;
    public byte id;

    public VoxelMod () {

        position = new ChunkVoxel();
        id = 0;

    }

    public VoxelMod (ChunkVoxel _position, byte _id) {

        position = _position;
        id = _id;

    }

}