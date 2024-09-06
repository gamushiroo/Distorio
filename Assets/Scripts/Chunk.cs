using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class Chunk {
    private bool isTerrainMapGenerated = false;
    private bool threadLocked = false;
    private bool isActive = true;
    private readonly byte[,,] voxelMap = new byte[Data.ChunkWidth, Data.ChunkHeight, Data.ChunkWidth];
    private readonly Queue<VoxelMod> modifications = new();
    private readonly List<Vector3> vertices = new();
    private readonly GameObject chunkObject = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();
    private readonly MeshFilter meshFilter;
    private readonly Vector2Int pos;
    private readonly World world;
    public Chunk (Vector2Int pos, World world) {
        this.pos = pos;
        this.world = world;
        chunkObject.transform.parent = world.transform;
        chunkObject.transform.position = new Vector3Int(pos.x, 0, pos.y) * Data.ChunkWidth;
        chunkObject.AddComponent<MeshRenderer>().material = world.material;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        GenerateTerrainData();
    }
    public bool IsEditable => !(!isTerrainMapGenerated || threadLocked);
    public byte GetVoxelIDChunk (Vector3Int pos) {
        return voxelMap[pos.x, pos.y, pos.z];
    }
    public void EnqueueVoxelMod (VoxelMod voxelMod) {
        modifications.Enqueue(voxelMod);
    }
    public void SetActiveState (bool value) {
        if (isActive != value) {
            chunkObject.SetActive(value);
            isActive = value;
        }
    }
    public void UpdateChunk () {
        if (Data.IsThread) {
            threadLocked = true;
            new Thread(new ThreadStart(UC)).Start();
        } else {
            UC();
        }
    }
    public void GenerateTerrainData () {
        if (Data.IsThread) {
            threadLocked = true;
            new Thread(new ThreadStart(GTD)).Start();
        } else {
            GTD();
        }
    }
    public void GenerateMesh () {
        int vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        for (int x = 0; x < Data.ChunkWidth; x++) {
            for (int y = 0; y < Data.ChunkHeight; y++) {
                for (int z = 0; z < Data.ChunkWidth; z++) {
                    if (voxelMap[x, y, z] != 0) {
                        if (world.blockTypes[voxelMap[x, y, z]].isGrass) {
                            for (int p = 0; p < 4; p++) {
                                for (int i = 0; i < 4; i++) {
                                    vertices.Add(Data.voxelVerts[Data.ffff[p, i]] + new Vector3(x, y, z));
                                    uvs.Add((Data.voxelUVs[i] + Data.TexturePos(world.blockTypes[voxelMap[x, y, z]].GetTextureID(p))) / Data.TextureSize);
                                }
                                for (int i = 0; i < 6; i++) {
                                    triangles.Add(Data.order[i] + vertexIndex);
                                }
                                vertexIndex += 4;
                            }
                        } else {
                            for (int p = 0; p < 6; p++) {
                                Vector3Int pos = Data.faceChecks[p] + new Vector3Int(x, y, z);
                                if (!(IsInChunk(pos) && world.blockTypes[GetVoxelIDChunk(pos)].isSolid)) {
                                    for (int i = 0; i < 4; i++) {
                                        vertices.Add(Data.voxelVerts[Data.voxelTris[p, i]] + new Vector3(x, y, z));
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
                }
            }
        }
        Mesh mesh = new() {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }
    private bool IsInChunk (Vector3 pos) {
        return !(pos.x < 0 || pos.x >= Data.ChunkWidth || pos.y < 0 || pos.y >= Data.ChunkHeight || pos.z < 0 || pos.z >= Data.ChunkWidth);
    }
    private void UC () {
        lock (modifications) {
            while (modifications.Count > 0) {
                VoxelMod vmod = modifications.Dequeue();
                voxelMap[vmod.pos.v.x, vmod.pos.v.y, vmod.pos.v.z] = vmod.id;
            }
        }
        threadLocked = false;
    }
    private void GTD () {
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