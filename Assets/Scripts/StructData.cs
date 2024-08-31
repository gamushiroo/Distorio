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
[System.Serializable]
public struct AABBData {

    public AABBData (float entryTime, Vector3 normal) {

        this.entryTime = entryTime;
        this.normal = normal;

    }

    public float entryTime;
    public Vector3 normal;

}

public struct Func {
    public ItemData itemIn;
    public ItemData itemOut;
    public Func(ItemData itemIn, ItemData itemOut) {
        this.itemIn = itemIn;
        this.itemOut = itemOut;
    }
}

public struct ItemData {
    public byte value;
    public byte id;
    public ItemData (byte value, byte id) {
        this.value = value;
        this.id = id;
    }
}

public struct Type {
    public byte value;
    public byte id;
    public Type (byte value, byte id) {
        this.value = value;
        this.id = id;
    }
}


public struct ChunkVoxel {
    public ChunkVoxel (Vector2Int c, Vector3Int v) {
        this.c = c;
        this.v = v;
    }
    public Vector2Int c;
    public Vector3Int v;
}