using UnityEngine;

public struct VoxelAndPos {
    public ChunkVoxel pos;
    public int id;
    public VoxelAndPos (ChunkVoxel pos, int id) {
        this.pos = pos;
        this.id = id;
    }
}
[System.Serializable]
public struct ItemType {
    public string itemName;
    public Sprite sprite;
    public float mineSpeed;
}
public struct ChunkVoxel {
    public Vector2Int c;
    public Vector3Int v;
    public ChunkVoxel (Vector2Int c, Vector3Int v) {
        this.c = c;
        this.v = v;
    }
}
