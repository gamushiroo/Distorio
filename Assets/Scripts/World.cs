using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    [SerializeField] private BiomeAttributes[] biome;
    public Material material;
    public List<BlockType> blockTypes = new();
    public Dictionary<Vector2Int, Chunk> chunks = new();
    public Queue<Queue<VoxelMod>> modifications = new();

    private bool _inUI;
    private int seed;
    private readonly List<Chunk> chunksToUpdate = new();
    private readonly List<Vector2Int> generatedChunks = new();
    private readonly Queue<Chunk> chunksToDraw = new();
    private List<Vector2Int> previouslyActiveChunks;

    private void Awake () {

        seed = (int)System.DateTime.Now.Ticks;
        Random.InitState(seed);
        CheckViewDistance(Vector2Int.zero);

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
                if (chunks[Vector2Int.zero].voxelMap[0, y, 0] != 0) {
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
        byte biomeType = 0;
        float terrainHeight = 0;


        for (int i =  0; i < 4; i++) {
            float a = Noise.Get2DPerlin(new(pos.x, pos.z), 0.05f) / 5000 + 1;
            terrainHeight += biome[biomeType].terrainHeight * Noise.Get2DPerlin(new(pos.x, pos.z), biome[biomeType].terrainScale / Mathf.Pow(2, i) / a);
        }
        terrainHeight *= Mathf.Pow(2, Noise.Get2DPerlin(new(pos.x, pos.z), 0.005f) * 4);
        //terrainHeight *= Noise.Get2DPerlin(new(pos.x, pos.z), 0.04f) + 0.5f;
        /*
        float eee = 0;
        for (int i = 0; i < 4; i++) {
            float a = Noise.Get2DPerlin(new(pos.x, pos.z), 0.05f / 2) / 5000 / 2 + 1;
            eee += biome[biomeType].terrainHeight * Noise.Get2DPerlin(new(pos.x, pos.z), biome[biomeType].terrainScale / Mathf.Pow(2, i) / a / 2) * 2;
        }
        eee *= Mathf.Pow(2, Noise.Get2DPerlin(new(pos.x, pos.z), 0.0025f) * 8);
        //terrainHeight *= Noise.Get2DPerlin(new(pos.x, pos.z), 0.02f) + 0.5f;

        */
        terrainHeight += biome[biomeType].solidGroundHeight;
        terrainHeight = Mathf.FloorToInt(terrainHeight);

        if (pos.y < terrainHeight) {


            VoxelValue = 1;
        }

        if (pos.y == terrainHeight) {
            if (Noise.Get2DPerlin(new(pos.x, pos.z), biome[biomeType].treeZoneScale) > biome[biomeType].treeZoneThreshold || terrainHeight >= 72) {
                if (Noise.Get2DPerlin(new(pos.x + 50, pos.z + 50), biome[biomeType].treePlacementScale) > biome[biomeType].treePlacementThreshold) {
                    lock (modifications) {
                        modifications.Enqueue(Structure.MakeTree(pos));
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
}

[System.Serializable]
public class BlockType {

    public string blockName;
    public bool isSolid;
    public bool hasCollision;
    public float hardness;
    public Sprite sprite;

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