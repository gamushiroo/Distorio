using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class Chunk {
    private readonly World world;
    private readonly MeshFilter meshFilter;
    private bool isTerrainMapGenerated;
    private bool threadLocked = true;
    private bool isActive = true;
    private readonly int[,,] voxelMap = new int[Data.ChunkWidth, Data.ChunkHeight, Data.ChunkWidth];
    private readonly Dictionary<Vector3Int, Inventory> Inventories = new();
    private readonly Queue<VoxelAndPos> modifications = new();
    private readonly List<Vector3> vertices = new();
    private readonly GameObject chunkObject = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();
    public bool IsEditable => isTerrainMapGenerated && !threadLocked;
    public Chunk (Vector2Int pos, World world) {
        this.world = world;
        chunkObject.transform.parent = world.transform;
        chunkObject.transform.position = Data.ChunkWidth * new Vector3Int(pos.x, 0, pos.y);
        chunkObject.AddComponent<MeshRenderer>().material = world.material;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        new Thread(new ThreadStart(GenerateTerrainData)).Start();
        void GenerateTerrainData () {
            for (int x = 0; x < Data.ChunkWidth; x++) {
                for (int y = 0; y < Data.ChunkHeight; y++) {
                    for (int z = 0; z < Data.ChunkWidth; z++) {
                        voxelMap[x, y, z] = world.GetVoxel(new Vector3Int(x + pos.x * Data.ChunkWidth, y, z + pos.y * Data.ChunkWidth));
                    }
                }
            }
            isTerrainMapGenerated = true;
            threadLocked = false;
        }
    }
    public int GetVoxelIDChunk (Vector3Int pos) {
        return pos.x >= 0 && pos.x < Data.ChunkWidth && pos.y >= 0 && pos.y < Data.ChunkHeight && pos.z >= 0 && pos.z < Data.ChunkWidth ? voxelMap[pos.x, pos.y, pos.z] : 0;
    }
    public bool GetInventory (Vector3Int pos) {
        return Inventories.ContainsKey(pos);
    }
    public void EnqueueVoxelMod (VoxelAndPos voxelMod) {
        modifications.Enqueue(voxelMod);
    }
    public void SetActiveState (bool value) {
        if (isActive != value) {
            isActive = value;
            chunkObject.SetActive(value);
        }
    }
    public void UpdateChunk () {
        lock (modifications) {
            while (modifications.Count > 0) {
                VoxelAndPos vmod = modifications.Dequeue();
                voxelMap[vmod.pos.v.x, vmod.pos.v.y, vmod.pos.v.z] = vmod.id;
                if (world.blockTypes[vmod.id].hasInventory) {
                    Inventories.Add(vmod.pos.v, new Inventory());
                }
            }
        }
        threadLocked = true;
        new Thread(new ThreadStart(AAA)).Start();
        void AAA () {
            int vertexIndex = 0;
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
            for (int x = 0; x < Data.ChunkWidth; x++) {
                for (int y = 0; y < Data.ChunkHeight; y++) {
                    for (int z = 0; z < Data.ChunkWidth; z++) {
                        if (voxelMap[x, y, z] != 0) {

                            switch (world.blockTypes[voxelMap[x, y, z]].meshTypes) {
                                case 0:
                                    NormalMesh(x, y, z);
                                    break;
                                case 1:
                                    GrassMesh(x, y, z);
                                    break;
                                case 2:
                                    HalfMesh(x, y, z);
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
            void NormalMesh (int x, int y, int z) {
                for (int p = 0; p < 6; p++) {
                    if (!world.blockTypes[GetVoxelIDChunk(Data.faceChecks[p] + new Vector3Int(x, y, z))].isSolid) {
                        for (int i = 0; i < 4; i++) {
                            vertices.Add(Data.voxelVerts[Data.blockMesh[p, i]] + new Vector3(x, y, z));
                            uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
                        }
                        for (int i = 0; i < 6; i++) {
                            triangles.Add(Data.order[i] + vertexIndex);
                        }
                        vertexIndex += 4;
                    }
                }
            }
            void HalfMesh (int x, int y, int z) {
                for (int p = 0; p < 6; p++) {
                    if (p == 2 || !world.blockTypes[GetVoxelIDChunk(Data.faceChecks[p] + new Vector3Int(x, y, z))].isSolid) {
                        for (int i = 0; i < 4; i++) {
                            vertices.Add(Data.halfVoxelVerts[Data.blockMesh[p, i]] + new Vector3(x, y, z));
                            uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
                        }
                        for (int i = 0; i < 6; i++) {
                            triangles.Add(Data.order[i] + vertexIndex);
                        }
                        vertexIndex += 4;
                    }
                }
            }
            void GrassMesh (int x, int y, int z) {
                for (int p = 0; p < 4; p++) {
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(Data.voxelVerts[Data.grassMesh[p, i]] + new Vector3(x, y, z));
                        uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
                    }
                    for (int i = 0; i < 6; i++) {
                        triangles.Add(Data.order[i] + vertexIndex);
                    }
                    vertexIndex += 4;
                }
            }
        }
    }
    public void GenerateMesh () {
        Mesh mesh = new() {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }
}