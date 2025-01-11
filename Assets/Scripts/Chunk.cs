using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk {
    public bool IsEditable => IsTerrainMapGenerated && !threadLocked;
    public bool IsTerrainMapGenerated { get; private set; }

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
    private readonly Vector2Int pos;
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





                        byte a = chunkManager.GetVoxel(x + pos.x * ChunkWidth, y, z + pos.y * ChunkWidth);
                        voxelMap[x, y, z] = a;
                    }
                }
            }
            IsTerrainMapGenerated = true;
            threadLocked = false;
        }
    }
    public int GetVoxelIDChunk (Vector3Int pos) {
        return pos.x >= 0 && pos.x < ChunkWidth && pos.y >= 0 && pos.y < ChunkHeight && pos.z >= 0 && pos.z < ChunkWidth ? voxelMap[pos.x, pos.y, pos.z] : 0;
    }
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
        chunkManager.QueueDraw1(pos);
    }

    public void Draw1 () {
        threadLocked = true;
        new Thread(new ThreadStart(UU)).Start();
        void UU () {
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
            threadLocked = false;
        }
    }
    public void Draw2 () {
        Mesh mesh = new() {
            subMeshCount = 2,
            vertices = vertices.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(waterTriangles.ToArray(), 1);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    bool IsOutOfChunk (Vector3Int pos) {
        return pos.x < 0 || pos.x >= ChunkWidth || pos.y < 0 || pos.y >= ChunkHeight || pos.z < 0 || pos.z >= ChunkWidth;
    }
    void NormalMesh (int x, int y, int z) {
        for (int p = 0; p < 6; p++) {

            int faceCheck = IsOutOfChunk(Data.faceChecks[p] + new Vector3Int(x, y, z)) ? chunkManager.GetVoxelID(new Vector3Int(pos.x * ChunkWidth + x, 0 + y, pos.y * ChunkWidth + z) + Data.faceChecks[p]) : GetVoxelIDChunk(Data.faceChecks[p] + new Vector3Int(x, y, z));

            if (voxelMap[x, y, z] == 7) {
                if (faceCheck != 7) {
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