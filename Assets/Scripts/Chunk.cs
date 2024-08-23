using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk {

    public byte[,,] voxelMap = new byte[Data.ChunkWidth, Data.ChunkHeight, Data.ChunkWidth];
    public Queue<VoxelMod> modifications = new();
    private bool threadLocked = false;
    private bool isTerrainMapGenerated = false;
    private bool isActive = true;
    private int vertexIndex = 0;
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();
    private readonly GameObject chunkObject;
    private readonly MeshFilter meshFilter;
    private readonly World world;
    private readonly Vector2Int pos;

    public Chunk (Vector2Int pos, World world, bool generateOnLoad) {
        this.pos = pos;
        this.world = world;
        chunkObject = new();
        chunkObject.transform.parent = world.transform;
        chunkObject.transform.position = new Vector3Int(pos.x, 0, pos.y) * Data.ChunkWidth;
        chunkObject.AddComponent<MeshRenderer>().material = world.material;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        if (generateOnLoad)
            GenerateTerrainData();
    }

    public void UpdateChunk () {
        threadLocked = true;
        new Thread(new ThreadStart(UC)).Start();
    }
    public bool IsEditable () {
        return !(!isTerrainMapGenerated || threadLocked);
    }
    public void GenerateTerrainData () {
        threadLocked = true;
        new Thread(new ThreadStart(GTD)).Start();
    }
    public void GenerateMesh () {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        for (int x = 0; x < Data.ChunkWidth; x++) {
            for (int y = 0; y < Data.ChunkHeight; y++) {
                for (int z = 0; z < Data.ChunkWidth; z++) {
                    if (voxelMap[x, y, z] != 0) {
                        for (int p = 0; p < 6; p++) {
                            Vector3Int pos = Data.faceChecks[p] + new Vector3Int(x, y, z);
                            if (!(Data.IsInChunk(pos) && world.blockTypes[voxelMap[pos.x, pos.y, pos.z]].isSolid)) {
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
        Mesh mesh = new() {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }
    public void SetActiveState(bool value) {
        if (isActive == value)
            return;
        chunkObject.SetActive(value);
        isActive = value;
    }
    private void UC () {
        lock (modifications) {
            while (modifications.Count > 0) {
                VoxelMod vmod = modifications.Dequeue();
                if (voxelMap[vmod.pos.v.x, vmod.pos.v.y, vmod.pos.v.z] == 0 || vmod.id != 5) {
                    voxelMap[vmod.pos.v.x, vmod.pos.v.y, vmod.pos.v.z] = vmod.id;
                }
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