using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public class Chunk {
    //Chunks‚©‚ç‚Ì‚Ý—˜—p‚³‚ê‚é
    public bool IsEditable => IsTerrainMapGenerated && !threadLocked;
    public bool IsTerrainMapGenerated { get; private set; }
    public readonly Vector3Int chunkPos;
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
    public Chunk (Vector3Int posIn) {
        chunkPos = ChunkWidth * posIn;
        chunkObject.transform.position = chunkPos;
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
                        voxelMap[x, y, z] = Terrain.GetTerrain(x + chunkPos.x, y + chunkPos.y, z + chunkPos.z);
                        if (voxelMap[x, y, z] == 1) {
                            Chunks.PlantTrees(new(x + chunkPos.x, y + chunkPos.y, z + chunkPos.z));
                        }
                    }
                }
            }
            IsTerrainMapGenerated = true;
            threadLocked = false;
        }
    }
    public void EnqueueVoxelMod (VoxelAndPos voxelMod) {
        modifications.Enqueue(voxelMod);
    }
    public void SetActiveState (bool value) {
        if (ActiveState != value) {
            chunkObject.SetActive(ActiveState = value);
        }
    }
    public void UpdateChunk () {
        lock (modifications) {
            while (modifications.Any()) {
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
                        CreateMeshAt(x, y, z);
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
    public int GetVoxelWithCheck (Vector3Int pos) {
        return pos.x >= 0 && pos.x < ChunkWidth && pos.y >= 0 && pos.y < ChunkHeight && pos.z >= 0 && pos.z < ChunkWidth ? GetVoxel(pos) : Chunks.GetVoxel(pos + chunkPos);
    }
    public int GetVoxel (Vector3Int pos) {
        return voxelMap[pos.x, pos.y, pos.z];
    }
    private void CreateMeshAt (int x, int y, int z) {
        Vector3Int pos = new(x, y, z);
        BlockType block = MyResources.blockTypes[voxelMap[x, y, z]];
        switch (block.RenderType) {
            case RenderType.none:
                break;
            case RenderType.standard:
                for (int f = 0; f < 6; f++) {
                    if (MyResources.blockTypes[GetVoxelWithCheck(faceChecks[f] + pos)].RenderType != block.RenderType) {
                        for (int i = 0; i < 4; i++) {
                            vertices.Add(MyResources.voxelVerts[MyResources.blockMesh[f, i]] + pos);
                            uvs.Add(MyResources.TexturePos(block.backFaceTexture, i));
                        }
                        for (int i = 0; i < 6; i++) {
                            triangles.Add(MyResources.order[i] + vertexIndex);
                        }
                        vertexIndex += 4;
                    }
                }
                break;
            case RenderType.liquid:
                for (int f = 0; f < 6; f++) {
                    if (MyResources.blockTypes[GetVoxelWithCheck(faceChecks[f] + pos)].RenderType != block.RenderType) {
                        for (int i = 0; i < 4; i++) {
                            vertices.Add(MyResources.voxelVerts[MyResources.blockMesh[f, i]] + pos);
                            uvs.Add(MyResources.TexturePos(block.backFaceTexture, i));
                        }
                        for (int i = 0; i < 6; i++) {
                            waterTriangles.Add(MyResources.order[i] + vertexIndex);
                        }
                        vertexIndex += 4;
                    }
                }
                break;
            case RenderType.notSolid:
                for (int f = 0; f < 6; f++) {
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(MyResources.voxelVerts[MyResources.blockMesh[f, i]] + pos);
                        uvs.Add(MyResources.TexturePos(block.backFaceTexture, i));
                    }
                    for (int i = 0; i < 6; i++) {
                        triangles.Add(MyResources.order[i] + vertexIndex);
                    }
                    vertexIndex += 4;
                }
                break;
        }
    }
    private static readonly Vector3Int[] faceChecks = new Vector3Int[6] {
        new(0, 0, -1),
        new(0, 0, 1),
        new(0, 1, 0),
        new(0, -1, 0),
        new(-1, 0, 0),
        new(1, 0, 0)
    };
}



/*
for (int p = 0; p < MyResources.grassMesh.Length >> 2; p++) {
    for (int i = 0; i < 4; i++) {
        vertices.Add(MyResources.voxelVerts[MyResources.grassMesh[p, i]] + pos);
        uvs.Add((MyResources.voxelUVs[i] + MyResources.TexturePos(MyResources.blockTypes[ttt].backFaceTexture)) / MyResources.TextureSize);
    }
    for (int i = 0; i < 6; i++) {
        triangles.Add(MyResources.order[i] + vertexIndex);
    }
    vertexIndex += 4;
}
*/