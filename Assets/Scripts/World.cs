using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    [SerializeField] private BiomeAttributes biome;
    public Material material;
    public List<BlockType> blockTypes = new();
    public List<ItemType> itemTypes = new();
    public Dictionary<Vector2Int, Chunk> chunks = new();
    public Queue<Queue<VoxelMod>> modifications = new();

    public Sprite slot;

    Vector2 offset;

    public Dictionary<ChunkVoxel, Func> funcs = new();

    private int seed;
    private readonly List<Chunk> chunksToUpdate = new();
    private readonly List<Vector2Int> generatedChunks = new();
    private readonly Queue<Chunk> chunksToDraw = new();
    private List<Vector2Int> previouslyActiveChunks;

    private void Awake () {
        offset = new(Random.Range(-66666.6f, 66666.6f), Random.Range(-66666.6f, 66666.6f));
        seed = (int)System.DateTime.Now.Ticks;
        Random.InitState(seed);

        for (int x = -8; x < 8; x++) {
            for (int y = -8; y < 8; y++) {

                Vector2Int pos = new(x, y);

                chunks.Add(pos, new(pos, this));
                generatedChunks.Add(pos);
                if (!chunksToUpdate.Contains(chunks[pos]))
                    chunksToUpdate.Add(chunks[pos]);


            }
        }
    }

    public void LateUpdate () {

        ApplyModifications();
        UpdateChunks();
        DrawChunks();

    }


    public byte GetVoxelID (ChunkVoxel pos) {
        return chunks[pos.c].GetVoxelIDChunk(pos.v);
    }
    private void DrawChunks () {
        if (chunksToDraw.Count > 0 && chunksToDraw.Peek().IsEditable)
            chunksToDraw.Dequeue().GenerateMesh();
    }

    public Vector3Int GetSpawnPoint () {
        Vector3Int pos = Vector3Int.zero;
        if (chunks.ContainsKey(Vector2Int.zero)) {
            for (int y = 0; y < Data.ChunkHeight; y++) {
                if (!blockTypes[GetVoxelID(new(Vector2Int.zero, new(0, y, 0)))].hasCollision) {
                    pos = new Vector3Int(0, y, 0);
                    break;
                }
            }
        }
        return pos;
    }

    void UpdateChunks () {
        if (chunksToUpdate.Count > 0) {
            for (int i = chunksToUpdate.Count - 1; i >= 0; i--) {

                if (chunksToUpdate[i].IsEditable) {
                    chunksToUpdate[i].UpdateChunk();
                    chunksToDraw.Enqueue(chunksToUpdate[i]);
                    chunksToUpdate.RemoveAt(i);

                }
            }
        }
    }

    public float GetTemp (Vector3 pos) {
        return (Get2DPerlin(new(pos.x, pos.z), 0.002f) + 0.5f) * 60 - 20;
    }
    float GetHeight (Vector3Int pos) {
        float terrainHeight = 0;
        for (int i = 0; i < 4; i++) {
            float a = Get2DPerlin(new(pos.x, pos.z), 0.05f) / 5000 + 1;
            terrainHeight += biome.terrainHeight * Get2DPerlin(new(pos.x, pos.z), biome.terrainScale / Mathf.Pow(2, i) / a);
        }
        float sec = Mathf.Pow(2, Get2DPerlin(new(pos.x, pos.z), 0.029f) * 4) + Mathf.Pow(2, Get2DPerlin(new(pos.x, pos.z), 0.005f) * 4);
        terrainHeight *= sec  / 2;
        terrainHeight = Mathf.FloorToInt(terrainHeight + biome.solidGroundHeight);
        return terrainHeight;
    }

    public byte GetVoxel (Vector3Int pos) {

        float terrainHeight = GetHeight(pos);
        float temp = GetTemp(pos);
        byte VoxelValue = 0;

        if (pos.y < terrainHeight) {

            if (pos.y == terrainHeight - 1) {
                if (temp < -5f) {
                    VoxelValue = 12;
                } else if (temp < 20f) {
                    VoxelValue = 1;
                } else {
                    VoxelValue = 6;
                }
            } else if (pos.y < terrainHeight - 1 && pos.y >= terrainHeight - 4) {
                VoxelValue = 2;
            } else {
                VoxelValue = 3;
            }
        }

        if (pos.y == terrainHeight) {
            lock (modifications) {
                int add = 0;
                byte type = 0;
                if (temp < -5f) {
                } else if (temp < 20f) {
                    add = 32;
                    type = 15;
                } else {
                    add = 128;
                    type = 14;
                }
                float www = Mathf.Max(0, Get2DPerlin(new(pos.x, pos.z), 0.052f) + 0.5f) * add;


                if (Get2DPerlin(new(pos.x, pos.z), 0.0158f) > 0) {
                    if (new System.Random().Next(0, Mathf.FloorToInt(www) + 2) == 0) {
                        Queue<VoxelMod> a = new();
                        a.Enqueue(new(Data.Vector3ToChunkVoxel(pos + Vector3.one * 0.5f), type));
                        modifications.Enqueue(a);
                    }
                }

            }
            if (Get2DPerlin(new(pos.x, pos.z), biome.treeZoneScale) > biome.treeZoneThreshold) {
                if (Get2DPerlin(new(pos.x + 50, pos.z + 50), biome.treePlacementScale) > biome.treePlacementThreshold) {
                    lock (modifications) {

                        if (temp < -5f) {
                        } else if (temp < 20f) {
                            if (new System.Random().Next(0, 4) == 0) {
                                modifications.Enqueue(Structure.MakeTreeTwo(pos));
                            } else {
                                modifications.Enqueue(Structure.MakeTree(pos));
                            }
                        } else {
                            modifications.Enqueue(Structure.MakeCactus(pos));
                        }
                    }
                }
            }
        }

        return VoxelValue;

    }

    public void CheckViewDistance (Vector2Int playerPos) {

        previouslyActiveChunks = new(generatedChunks);
        for (int x = playerPos.x - Data.ChunkLoadRange; x < playerPos.x + Data.ChunkLoadRange; x++) {
            for (int y = playerPos.y - Data.ChunkLoadRange; y < playerPos.y + Data.ChunkLoadRange; y++) {

                Vector2Int pos = new(x, y);

                if (!chunks.ContainsKey(pos)) {

                    chunks.Add(pos, new(pos, this));
                    generatedChunks.Add(pos);

                    if (!chunksToUpdate.Contains(chunks[pos]))
                        chunksToUpdate.Add(chunks[pos]);

                } else {
                    chunks[pos].SetActiveState(true);
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++) {



                    if (previouslyActiveChunks[i] == pos) {

                        previouslyActiveChunks.RemoveAt(i);

                    }
                }
            }
        }

        foreach (Vector2Int c in previouslyActiveChunks) {
            chunks[c].SetActiveState(false);

        }
        previouslyActiveChunks.Clear();
    }

    void ApplyModifications () {

        lock (modifications) {
            while (modifications.Count > 0) {
                Queue<VoxelMod> queue = modifications.Dequeue();

                while (queue.Count > 0) {

                    VoxelMod vmod = queue.Dequeue();

                    if (!chunks.ContainsKey(vmod.pos.c)) {

                        chunks.Add(vmod.pos.c, new(vmod.pos.c, this));
                        generatedChunks.Add(vmod.pos.c);

                    }
                    chunks[vmod.pos.c].EnqueueVoxelMod(vmod);
                    if (!chunksToUpdate.Contains(chunks[vmod.pos.c]))
                        chunksToUpdate.Add(chunks[vmod.pos.c]);
                }
            }
        }
    }


    public float Get2DPerlin (Vector2 position, float scale) {

        return Mathf.PerlinNoise(position.x * scale + offset.x, position.y * scale + offset.y) - 0.5f;

        //heigh += Mathf.Clamp(Mathf.PerlinNoise(pos.x * 0.01f, pos.y * 0.01f) * 256 - 192, -16, 16);

    }
}

[System.Serializable]
public class BlockType {

    public string blockName;
    public bool isSolid;
    public bool hasCollision;
    public float hardness;
    public Sprite sprite;
    public bool isGrass;

    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID (int faceIndex) {
        return faceIndex switch {
            0 => backFaceTexture,
            1 => frontFaceTexture,
            2 => topFaceTexture,
            3 => bottomFaceTexture,
            4 => leftFaceTexture,
            5 => rightFaceTexture,
            _ => 0,
        };
    }
}

[System.Serializable]
public class ItemType {

    public string itemName;
    public Sprite sprite;
    public float mineSpeed;


}