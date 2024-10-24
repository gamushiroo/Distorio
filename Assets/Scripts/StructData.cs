using System.CodeDom.Compiler;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;



public struct BlockAndSelect {
    public BlockAndSelect (ChunkVoxel blue, ChunkVoxel red) {

        this.blue = blue;
        this.red = red;

    }
    public ChunkVoxel blue;
    public ChunkVoxel red;
}

public struct EntityData {
    public EntityData (string name, Vector3 size, float maxHealth, float maxEnergy) {

        this.name = name;
        this.size = size;
        this.health = maxHealth;
        this.maxEnergy = maxEnergy;

    }
    public string name;
    public Vector3 size;
    public float health;
    public float maxEnergy;
}
[System.Serializable]
public struct WWWEe {
    public WWWEe (Vector3 pos, Vector3 size, Vector3 vel, bool isGrounded) {

        this.pos = pos;
        this.size = size;
        this.vel = vel;
        this.isGrounded = isGrounded;

        aabbData = new AABBData();
    }

    public AABBData aabbData;
    public Vector3 pos;
    public Vector3 size;
    public Vector3 vel;
    public bool isGrounded;
}

public struct VoxelAndPos {

    public ChunkVoxel pos;
    public byte id;

    public VoxelAndPos (ChunkVoxel _position, byte _id) {

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

    public static ChunkVoxel zero = new ChunkVoxel(Vector2Int.zero, Vector3Int.zero);

    public Vector2Int c;
    public Vector3Int v;
}
