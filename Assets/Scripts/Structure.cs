using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    public static Queue<VoxelAndPos> MakeCactus (Vector3Int position) {

        Queue<VoxelAndPos> queue = new();

        for (int _y = 0; _y < 4; _y++) {
            queue.Enqueue(new(Data.Vector3ToChunkVoxel(new Vector3Int(0, _y, 0) + position), 11));
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
                        queue.Enqueue(new(Data.Vector3ToChunkVoxel(pos + new Vector3Int(-2, 0, -2)), Data.trees[_x, _y, _z]));
                    }
                }
            }
        }
        return queue;
    }
    public static Queue<VoxelAndPos> MakeCave (Vector3Int position) {
        Queue<VoxelAndPos> queue = new();
        int range = 4;
        Quaternion currentRot = Quaternion.identity;
        List<Vector3> aaa = new();
        for (int i = 0; i < 64; i++) {
            currentRot *= Quaternion.Euler(new(new System.Random().Next(-15, 15), 0, new System.Random().Next(-15, 15)));

            aaa.Add(position + currentRot * new Vector3(0, -i * 3, 0));
        }
        for (int i = 0; i < aaa.Count; i++) {

            for (int x = -range; x < range; x++) {
                for (int y = -range; y < range; y++) {
                    for (int z = -range; z < range; z++) {

                        Vector3 aee = aaa[i] + new Vector3Int(x, y, z);
                        if(Mathf.Abs((aee - aaa[i]).magnitude) < range) {
                            queue.Enqueue(new(Data.Vector3ToChunkVoxel(aee), 0));
                        }
                    }
                }
            }
        }
        return queue;
    }
}