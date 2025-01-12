using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk {
    public bool IsEditable => IsTerrainMapGenerated && !threadLocked;
    public bool IsTerrainMapGenerated { get; private set; }
    public readonly Vector2Int pos;

    private static readonly int ChunkWidth = 16;
    private static readonly int ChunkHeight = 128;
    private bool threadLocked;
    private bool ActiveState = true;
    private readonly ChunkManager chunkManager;
    private readonly MeshFilter meshFilter;
    private readonly GameObject chunkObject = new();
    private readonly Queue<VoxelAndPos> modifications = new();
    private readonly int[,,] voxelMap = new int[ChunkWidth, ChunkHeight, ChunkWidth];
    private readonly List<Vector3> vertices = new();
    private readonly List<Vector2> uvs = new();
    private readonly List<int> triangles = new();
    private readonly List<int> waterTriangles = new();
    private int vertexIndex = 0;
    public Chunk (Vector2Int pos, ChunkManager chunkManager, Material[] materials) {
        this.chunkManager = chunkManager;
        this.pos = pos;
        chunkObject.transform.position = ChunkWidth * new Vector3Int(pos.x, 0, pos.y);
        chunkObject.AddComponent<MeshRenderer>().materials = materials;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
    }

    public void GenerateTerrainData () {
        threadLocked = true;
        new Thread(new ThreadStart(GenerateTerrainData)).Start();
        void GenerateTerrainData () {
            for (int x = 0; x < ChunkWidth; x++) {
                for (int y = 0; y < ChunkHeight; y++) {
                    for (int z = 0; z < ChunkWidth; z++) {
                        voxelMap[x, y, z] = GetTerrain(x + pos.x * ChunkWidth, y, z + pos.y * ChunkWidth);
                        if (voxelMap[x, y, z] == 1) {
                            chunkManager.EETT(new(x + pos.x * ChunkWidth, y, z + pos.y * ChunkWidth));
                        }
                    }
                }
            }
            IsTerrainMapGenerated = true;
            threadLocked = false;
        }
    }
    private static byte GetTerrain (int x, int y, int z) {
        byte VoxelValue = y < 40 ? (byte)4 : (byte)0;
        switch (y - Mathf.FloorToInt(GetTerrainHeight(x, z))) {
            case < -4:
                VoxelValue = 3;
                break;
            case < -1:
                VoxelValue = 2;
                break;
            case < 0:
                VoxelValue = 1;
                break;
            default:
                break;
        }
        return VoxelValue;
    }
    private static float GetTerrainHeight (int x, int y) {
        Vector2 pos = new(x, y);
        float terrainHeight = 0;
        for (int i = 0; i < 4; i++) {
            terrainHeight += Noise.Get2DPerlin(pos, Data.terrainScale / Mathf.Pow(2, i));
        }
        return terrainHeight * Data.terrainHeight * (Mathf.Pow(2, Noise.Get2DPerlin(pos, 0.029F) * 4) + Mathf.Pow(2, Noise.Get2DPerlin(pos, 0.005F) * 4) / 2) / (Noise.Get2DPerlin(pos, 0.05f) / 5000 + 1) + Data.solidGroundHeight;
    }


    public int GetVoxelIDChunk (Vector3Int pos) => pos.x >= 0 && pos.x < ChunkWidth && pos.y >= 0 && pos.y < ChunkHeight && pos.z >= 0 && pos.z < ChunkWidth ? voxelMap[pos.x, pos.y, pos.z] : 0;

    public void EnqueueVoxelMod (VoxelAndPos voxelMod) {
        modifications.Enqueue(voxelMod);
    }
    public void SetActiveState (bool value) {
        if (ActiveState != value) {
            ActiveState = value;
            chunkObject.SetActive(value);
        }
    }

    public void UpdateChunk () {
        lock (modifications) {
            while (modifications.Count > 0) {
                VoxelAndPos vmod = modifications.Dequeue();
                voxelMap[vmod.pos.v.x, vmod.pos.v.y, vmod.pos.v.z] = vmod.id;
            }
        }
    }

    public async void GenerateMesh () {
        threadLocked = true;
        await Task.Run(() => {
            vertexIndex = 0;
            vertices.Clear();
            triangles.Clear();
            waterTriangles.Clear();
            uvs.Clear();
            for (int x = 0; x < ChunkWidth; x++) {
                for (int y = 0; y < ChunkHeight; y++) {
                    for (int z = 0; z < ChunkWidth; z++) {
                        if (voxelMap[x, y, z] != 0) {
                            switch (Data.blockTypes[voxelMap[x, y, z]].meshTypes) {
                                case 0:
                                    NormalMesh(x, y, z);
                                    break;
                                case 1:
                                    GrassMesh(x, y, z, Data.grassMesh);
                                    break;
                                default:
                                    NormalMesh(x, y, z);
                                    break;
                            }
                        }
                    }
                }
            }
        });
        Mesh mesh = new() {
            subMeshCount = 2,
            vertices = vertices.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(waterTriangles.ToArray(), 1);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        threadLocked = false;
    }




    bool IsOutOfChunk (Vector3Int pos) {
        return pos.x < 0 || pos.x >= ChunkWidth || pos.y < 0 || pos.y >= ChunkHeight || pos.z < 0 || pos.z >= ChunkWidth;
    }


    void NormalMesh (int x, int y, int z) {
        for (int p = 0; p < 6; p++) {

            int faceCheck = IsOutOfChunk(Data.faceChecks[p] + new Vector3Int(x, y, z)) ? chunkManager.GetVoxelID(new Vector3Int(pos.x * ChunkWidth + x, 0 + y, pos.y * ChunkWidth + z) + Data.faceChecks[p]) : GetVoxelIDChunk(Data.faceChecks[p] + new Vector3Int(x, y, z));

            if (voxelMap[x, y, z] == 4) {
                if (faceCheck != 4) {
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(Data.voxelVerts[Data.blockMesh[p, i]] + new Vector3(x, y, z));
                        uvs.Add((Data.voxelUVs[i] + Data.TexturePos(Data.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
                    }
                    for (int i = 0; i < 6; i++) {
                        waterTriangles.Add(Data.order[i] + vertexIndex);
                    }
                    vertexIndex += 4;
                }
            } else {
                if (!Data.blockTypes[faceCheck].isSolid) {
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(Data.voxelVerts[Data.blockMesh[p, i]] + new Vector3(x, y, z));
                        uvs.Add((Data.voxelUVs[i] + Data.TexturePos(Data.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
                    }
                    AddTriangles();
                }
            }
        }
    }
    void GrassMesh (int x, int y, int z, int[,] mesh) {
        for (int p = 0; p < mesh.Length >> 2; p++) {
            for (int i = 0; i < 4; i++) {
                vertices.Add(Data.voxelVerts[mesh[p, i]] + new Vector3(x, y, z));
                uvs.Add((Data.voxelUVs[i] + Data.TexturePos(Data.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
            }
            AddTriangles();
        }
    }
    void AddTriangles () {
        for (int i = 0; i < 6; i++) {
            triangles.Add(Data.order[i] + vertexIndex);
        }
        vertexIndex += 4;
    }
}