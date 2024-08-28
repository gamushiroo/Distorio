using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    [SerializeField] private BiomeAttributes biome;
    public Material material;
    public List<BlockType> blockTypes = new();
    public List<BlockType> itemTypes = new();
    public Dictionary<Vector2Int, Chunk> chunks = new();
    public Queue<Queue<VoxelMod>> modifications = new();

    Vector2 offset;

    public Dictionary<ChunkVoxel, Func> funcs = new();

    private bool _inUI;
    private int seed;
    private readonly List<Chunk> chunksToUpdate = new();
    private readonly List<Vector2Int> generatedChunks = new();
    private readonly Queue<Chunk> chunksToDraw = new();
    private List<Vector2Int> previouslyActiveChunks;

    private void Awake () {
        offset = new(Random.Range(-66666.6f, 66666.6f), Random.Range(-66666.6f, 66666.6f));
        seed = (int)System.DateTime.Now.Ticks;
        Random.InitState(seed);

        for (int x = -16; x < 16; x++) {
            for (int y = -16; y < 16; y++) {

                Vector2Int pos = new(x, y);

                chunks.Add(pos, new(pos, this, true));
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

    private void DrawChunks () {
        if (chunksToDraw.Count > 0 && chunksToDraw.Peek().IsEditable())
            chunksToDraw.Dequeue().GenerateMesh();
    }

    public Vector3Int GetSpawnPoint () {
        Vector3Int pos = Vector3Int.zero;
        if (chunks.ContainsKey(Vector2Int.zero)) {
            for (int y = Data.ChunkHeight - 1; y >= 0; y--) {
                if (blockTypes[chunks[Vector2Int.zero].voxelMap[0, y, 0]].hasCollision) {
                    pos = new Vector3Int(0, y + 1, 0);
                    break;
                }
            }
        }
        return pos;
    }

    void UpdateChunks () {
        if (chunksToUpdate.Count > 0) {
            for (int i = chunksToUpdate.Count - 1; i >= 0; i--) {

                if (chunksToUpdate[i].IsEditable()) {
                    chunksToUpdate[i].UpdateChunk();
                    chunksToDraw.Enqueue(chunksToUpdate[i]);
                    chunksToUpdate.RemoveAt(i);

                }
            }
        }
    }

    public bool InUI {
        get { return _inUI; }
        set { _inUI = value; }
    }

    public byte GetVoxel (Vector3Int pos) {

        byte VoxelValue = 0;
        float terrainHeight = 0;


        for (int i =  0; i < 4; i++) {
            float a = Get2DPerlin(new(pos.x, pos.z), 0.05f) / 5000 + 1;
            terrainHeight += biome.terrainHeight * Get2DPerlin(new(pos.x, pos.z), biome.terrainScale / Mathf.Pow(2, i) / a);
        }
        terrainHeight *= Mathf.Pow(2, Get2DPerlin(new(pos.x, pos.z), 0.005f) * 4);
        terrainHeight = Mathf.FloorToInt(terrainHeight + biome.solidGroundHeight);

        if (pos.y < terrainHeight) {

            if(Get2DPerlin(new(pos.x, pos.z), 0.005f) < 0f) {

                VoxelValue = 1;
            } else {
                VoxelValue = 6;

            }
        }
        if (pos.y == terrainHeight) {
            if (Get2DPerlin(new(pos.x, pos.z), biome.treeZoneScale) > biome.treeZoneThreshold || terrainHeight >= 72) {
                if (Get2DPerlin(new(pos.x + 50, pos.z + 50), biome.treePlacementScale) > biome.treePlacementThreshold) {
                    if (Get2DPerlin(new(pos.x, pos.z), 0.005f) < 0f) {
                        modifications.Enqueue(Structure.MakeTree(pos));
                    } else {

                        modifications.Enqueue(Structure.MakeCactus(pos));
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

                    chunks.Add(pos, new(pos, this, true));
                    generatedChunks.Add(pos);

                    if (!chunksToUpdate.Contains(chunks[pos]))
                        chunksToUpdate.Add(chunks[pos]);

                }
                else {
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

                        chunks.Add(vmod.pos.c, new(vmod.pos.c, this, true));
                        generatedChunks.Add(vmod.pos.c);

                    }
                    chunks[vmod.pos.c].modifications.Enqueue(vmod);
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

    public bool isFunc;

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

public class ItemType {

    public string itemName;
    public Sprite sprite;

}