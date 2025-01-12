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
    private readonly MeshFilter meshFilter;
    private readonly GameObject chunkObject = new();
    private readonly Queue<VoxelAndPos> modifications = new();
    private readonly byte[,,] voxelMap = new byte[ChunkWidth, ChunkHeight, ChunkWidth];
    private readonly List<Vector3> vertices = new();
    private readonly List<Vector2> uvs = new();
    private readonly List<int> triangles = new();
    private readonly List<int> waterTriangles = new();
    private int vertexIndex = 0;
    public Chunk (Vector2Int pos) {
        this.pos = pos;
        chunkObject.transform.position = ChunkWidth * new Vector3Int(pos.x, 0, pos.y);
        chunkObject.AddComponent<MeshRenderer>().materials = MyResources.materials;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
    }
    public void GenerateTerrainData () {
        threadLocked = true;
        new Thread(new ThreadStart(GenerateTerrainData)).Start();
        void GenerateTerrainData () {
            for (int x = 0; x < ChunkWidth; x++) {
                for (int y = 0; y < ChunkHeight; y++) {
                    for (int z = 0; z < ChunkWidth; z++) {
                        voxelMap[x, y, z] = Terrain.GetTerrain(x + pos.x * ChunkWidth, y, z + pos.y * ChunkWidth);
                        if (voxelMap[x, y, z] == 1) {
                            Chunks.PlantTrees(new(x + pos.x * ChunkWidth, y, z + pos.y * ChunkWidth));
                        }
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
                            switch (MyResources.blockTypes[voxelMap[x, y, z]].meshTypes) {
                                case 0:
                                    NormalMesh(x, y, z);
                                    break;
                                case 1:
                                    GrassMesh(x, y, z, MyResources.grassMesh);
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
            int faceCheck = IsOutOfChunk(MyResources.faceChecks[p] + new Vector3Int(x, y, z)) ? Chunks.GetVoxelID(new Vector3Int(pos.x * ChunkWidth + x, 0 + y, pos.y * ChunkWidth + z) + MyResources.faceChecks[p]) : GetVoxelIDChunk(MyResources.faceChecks[p] + new Vector3Int(x, y, z));
            if (voxelMap[x, y, z] == 4) {
                if (faceCheck != 4) {
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(MyResources.voxelVerts[MyResources.blockMesh[p, i]] + new Vector3(x, y, z));
                        uvs.Add((MyResources.voxelUVs[i] + MyResources.TexturePos(MyResources.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / MyResources.TextureSize);
                    }
                    for (int i = 0; i < 6; i++) {
                        waterTriangles.Add(MyResources.order[i] + vertexIndex);
                    }
                    vertexIndex += 4;
                }
            } else {
                if (!MyResources.blockTypes[faceCheck].isSolid) {
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(MyResources.voxelVerts[MyResources.blockMesh[p, i]] + new Vector3(x, y, z));
                        uvs.Add((MyResources.voxelUVs[i] + MyResources.TexturePos(MyResources.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / MyResources.TextureSize);
                    }
                    AddTriangles();
                }
            }
        }
    }
    void GrassMesh (int x, int y, int z, int[,] mesh) {
        for (int p = 0; p < mesh.Length >> 2; p++) {
            for (int i = 0; i < 4; i++) {
                vertices.Add(MyResources.voxelVerts[mesh[p, i]] + new Vector3(x, y, z));
                uvs.Add((MyResources.voxelUVs[i] + MyResources.TexturePos(MyResources.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / MyResources.TextureSize);
            }
            AddTriangles();
        }
    }
    void AddTriangles () {
        for (int i = 0; i < 6; i++) {
            triangles.Add(MyResources.order[i] + vertexIndex);
        }
        vertexIndex += 4;
    }
}