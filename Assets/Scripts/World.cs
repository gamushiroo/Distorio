using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    [SerializeField] private Player player;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private BiomeAttributes biome;
    public Material material;
    public List<BlockType> blockTypes = new();

    public Dictionary<Vector2Int, Chunk> chunks = new();
    public List<Vector2Int> generatedChunks = new();
    public Queue<Chunk> chunksToDraw = new();
    public Queue<Queue<VoxelMod>> modifications = new();

    private bool _inUI;
    private int seed;
    private Vector2Int playerChunkCoord;
    private Vector2Int playerLastChunkCoord;
    private List<Chunk> chunksToUpdate = new();
    private List<Vector2Int> previouslyActiveChunks = new();

    private void Awake () {

        seed = (int)System.DateTime.Now.Ticks;
        Random.InitState(seed);
        playerLastChunkCoord = Data.Vector3ToChunkVoxel(playerObject.transform.position).c;
        CheckViewDistance();
    }

    public void LateUpdate () {

        playerChunkCoord = Data.Vector3ToChunkVoxel(playerObject.transform.position).c;

        if (!( playerChunkCoord == playerLastChunkCoord )) {
            CheckViewDistance();
        }
        ApplyModifications();
        if (chunksToUpdate.Count > 0) {
            UpdateChunks();
        }
        if (chunksToDraw.Count > 0) {
            lock (chunksToDraw) {

                if (chunksToDraw.Peek().IsEditable()) {

                    chunksToDraw.Dequeue().GenerateMesh();

                }
            }
        }

        player.spawnPoint = CalculateSpawnPoint();

    }

    private Vector3Int CalculateSpawnPoint () {

        Vector3Int pos = Vector3Int.zero;

        if (chunks.ContainsKey(Vector2Int.zero)) {

            Chunk chunk = chunks[Vector2Int.zero];

            for (int y = Data.ChunkHeight - 1; y >= 0; y--) {
                if (chunk.voxelMap[0, y, 0].id != 0) {
                    pos = new Vector3Int(0, y + 1, 0);
                    break;
                }
            }

        }
        return pos;
    }

    void UpdateChunks () {

        for (int i = chunksToUpdate.Count - 1; i >= 0; i--) {

            if (chunksToUpdate[i].IsEditable()) {
                chunksToUpdate[i].UpdateChunkInThread();
                chunksToUpdate.RemoveAt(i);

            }
        }
    }

    public bool inUI {
        get { return _inUI; }
        set { _inUI = value; }
    }

    public byte GetVoxel(Vector3Int pos) {

        float terrainHeight = biome.solidGroundHeight;
        terrainHeight += biome.terrainHeight * Noise.Get2DPerlin(new(pos.x, pos.z), biome.terrainScale);
        terrainHeight += biome.terrainHeight2 * Noise.Get2DPerlin(new(pos.x, pos.z), biome.terrainScale2);
        terrainHeight = Mathf.FloorToInt(terrainHeight);

        byte VoxelValue;

        if (pos.y == terrainHeight)
            VoxelValue = 1;
        else if (pos.y > terrainHeight)
            return 0;
        else
            VoxelValue = 2;

        return VoxelValue;

    }

    void CheckViewDistance () {

        playerLastChunkCoord = playerChunkCoord;

        previouslyActiveChunks = new(generatedChunks);

        for (int x = playerChunkCoord.x - Data.ChunkLoadRange; x < playerChunkCoord.x + Data.ChunkLoadRange; x++) {
            for (int y = playerChunkCoord.y - Data.ChunkLoadRange; y < playerChunkCoord.y + Data.ChunkLoadRange; y++) {

                Vector2Int pos = new(x, y);

                if (!chunks.ContainsKey(pos)) {

                    chunks.Add(pos, new(pos, this, true));
                    if (!chunksToUpdate.Contains(chunks[pos]))
                        chunksToUpdate.Add(chunks[pos]);

                }
                else if (!chunks[pos].IsActive) {

                    chunks[pos].IsActive = true;

                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++) {

                    if (previouslyActiveChunks[i] == pos) {

                        previouslyActiveChunks.RemoveAt(i);

                    }
                }
            }
        }

        foreach (Vector2Int c in previouslyActiveChunks) {
            chunks[c].IsActive = false;

        }
        previouslyActiveChunks.Clear();
    }

    void ApplyModifications () {

        while (modifications.Count > 0) {

            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0) {

                VoxelMod vmod = queue.Dequeue();



                if (!chunks.ContainsKey(vmod.position.c)) {
                    chunks.Add(vmod.position.c, new(vmod.position.c, this, true));

                }
                else if (chunks[vmod.position.c].IsEditable()) {

                    chunks[vmod.position.c].modifications.Enqueue(vmod);

                    if (!chunksToUpdate.Contains(chunks[vmod.position.c]))
                        chunksToUpdate.Add(chunks[vmod.position.c]);

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

        switch (faceIndex) {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                return 0;

        }
    }
}