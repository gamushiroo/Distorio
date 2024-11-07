using UnityEngine;


public struct VoxelAndPos {

    public ChunkVoxel pos;
    public int id;

    public VoxelAndPos (ChunkVoxel _position, int _id) {

        pos = _position;
        id = _id;

    }
}

[System.Serializable]
public struct AABBData {

    public AABBData (float entryTime, Vector3 normal) {

        this.entryTime = entryTime;
        this.normal = normal;

    }

    public float entryTime;
    public Vector3 normal;

}

[System.Serializable]
public struct ItemType {
    public string itemName;
    public Sprite sprite;
    public float mineSpeed;
}


[System.Serializable]
public struct ChunkVoxel {
    public ChunkVoxel (Vector2Int c, Vector3Int v) {
        this.c = c;
        this.v = v;
    }

    public static bool Equal (ChunkVoxel a, ChunkVoxel b) {
        return a.c == b.c && a.v == b.v;
    }

    public static ChunkVoxel zero = new(Vector2Int.zero, Vector3Int.zero);

    public Vector2Int c;
    public Vector3Int v;
}
